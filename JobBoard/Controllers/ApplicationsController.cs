using JobBoard.Data;
using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private const int MaxMessageLength = 4000;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ApplicationsController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Employer(int? vacancyId)
    {
        var employerId = _users.GetUserId(User)!;
        var vacancies = await _db.Vacancies.Where(v => v.EmployerId == employerId)
            .OrderByDescending(v => v.UpdatedAt).ToListAsync();
        var query = _db.JobApplications
            .Include(a => a.Vacancy).Include(a => a.Resume).Include(a => a.Seeker).Include(a => a.Messages)
            .Where(a => a.Vacancy!.EmployerId == employerId);
        if (vacancyId.HasValue) query = query.Where(a => a.VacancyId == vacancyId.Value);
        return View(new EmployerApplicationsViewModel
        {
            Applications = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(),
            Vacancies = vacancies,
            VacancyId = vacancyId
        });
    }

    [HttpGet]
    public async Task<IActionResult> Dialogs()
    {
        var userId = _users.GetUserId(User)!;
        var query = _db.JobApplications
            .Include(a => a.Vacancy).ThenInclude(v => v!.Employer)
            .Include(a => a.Seeker).Include(a => a.Resume).Include(a => a.Messages)
            .Where(a => a.SeekerId == userId || a.Vacancy!.EmployerId == userId);
        var dialogs = await query.OrderByDescending(a => a.Messages.Max(m => (DateTime?)m.SentAt) ?? a.CreatedAt).ToListAsync();
        return View(dialogs);
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int vacancyId, int resumeId)
    {
        var userId = _users.GetUserId(User)!;

        var vacancy = await _db.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancyId && v.IsPublished);
        if (vacancy == null)
        {
            return NotFound();
        }

        var existing = await _db.JobApplications
            .FirstOrDefaultAsync(a => a.VacancyId == vacancyId && a.SeekerId == userId);
        if (existing != null)
        {
            return RedirectToAction(nameof(Chat), new { id = existing.Id });
        }

        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.SeekerId == userId);
        if (resume == null)
        {
            TempData["Error"] = "Выберите резюме для отклика";
            return RedirectToAction("Details", "Vacancies", new { id = vacancyId });
        }

        var application = new JobApplication
        {
            VacancyId = vacancyId,
            ResumeId = resumeId,
            SeekerId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Отклик отправлен";
        return RedirectToAction(nameof(Chat), new { id = application.Id });
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StartEmployerConversation(int vacancyId, int resumeId)
    {
        var employerId = _users.GetUserId(User)!;
        var vacancy = await _db.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancyId && v.EmployerId == employerId);
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.IsPublished);
        if (vacancy == null || resume == null) return NotFound();
        var application = await _db.JobApplications.FirstOrDefaultAsync(a => a.VacancyId == vacancyId && a.SeekerId == resume.SeekerId);
        if (application == null)
        {
            application = new JobApplication { VacancyId = vacancyId, ResumeId = resumeId, SeekerId = resume.SeekerId, CreatedAt = DateTime.UtcNow };
            _db.JobApplications.Add(application);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Chat), new { id = application.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Chat(int id)
    {
        var userId = _users.GetUserId(User)!;
        var application = await LoadParticipantApplication(id, userId);
        if (application == null)
        {
            return NotFound();
        }

        var model = new ChatViewModel
        {
            Application = application,
            CurrentUserId = userId,
            IsSeeker = application.SeekerId == userId,
            Messages = await _db.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.JobApplicationId == id)
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .ToListAsync()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Messages(int id)
    {
        var userId = _users.GetUserId(User)!;
        var application = await LoadParticipantApplication(id, userId);
        if (application == null) return NotFound();
        var model = new ChatViewModel
        {
            Application = application, CurrentUserId = userId, IsSeeker = application.SeekerId == userId,
            Messages = await _db.ChatMessages.Include(m => m.Sender).Where(m => m.JobApplicationId == id)
                .OrderBy(m => m.SentAt).ThenBy(m => m.Id).ToListAsync()
        };
        return PartialView("_Messages", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int id, string? text)
    {
        var userId = _users.GetUserId(User)!;
        var application = await LoadParticipantApplication(id, userId);
        if (application == null)
        {
            return NotFound();
        }

        text = text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            TempData["Error"] = "Сообщение не может быть пустым";
            return RedirectToAction(nameof(Chat), new { id });
        }

        if (text.Length > MaxMessageLength)
        {
            TempData["Error"] = $"Максимальная длина сообщения — {MaxMessageLength} символов";
            return RedirectToAction(nameof(Chat), new { id });
        }

        _db.ChatMessages.Add(new ChatMessage
        {
            JobApplicationId = id,
            SenderId = userId,
            Text = text,
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
        return RedirectToAction(nameof(Chat), new { id });
    }

    private async Task<JobApplication?> LoadParticipantApplication(int id, string userId)
    {
        var application = await _db.JobApplications
            .Include(a => a.Vacancy).ThenInclude(v => v!.Employer)
            .Include(a => a.Resume)
            .Include(a => a.Seeker)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return null;
        }

        var isParticipant = application.SeekerId == userId || application.Vacancy!.EmployerId == userId;
        return isParticipant ? application : null;
    }
}
