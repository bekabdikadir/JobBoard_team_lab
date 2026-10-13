using JobBoard.Data;
using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Controllers;

[Authorize]
public class ResumesController : Controller
{
    private const int PageSize = 12;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ResumesController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ResumeFormViewModel();
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResumeFormViewModel model)
    {
        model.Id = null;
        await ValidateCategory(model);
        if (!ModelState.IsValid)
        {
            await FillCategories(model);
            return View("Form", model);
        }

        var resume = new Resume
        {
            SeekerId = _users.GetUserId(User)!,
            CreatedAt = DateTime.UtcNow
        };
        Map(model, resume);
        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Резюме создано";
        return RedirectToAction(nameof(Details), new { id = resume.Id });
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _users.GetUserId(User)!;
        var resume = await _db.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == userId);
        if (resume == null)
        {
            return NotFound();
        }

        var model = new ResumeFormViewModel
        {
            Id = resume.Id,
            Title = resume.Title,
            CategoryId = resume.CategoryId,
            ExpectedSalary = resume.ExpectedSalary,
            Telegram = resume.Telegram,
            ContactEmail = resume.ContactEmail,
            Phone = resume.Phone,
            Facebook = resume.Facebook,
            LinkedIn = resume.LinkedIn,
            WorkExperiences = resume.WorkExperiences
                .OrderByDescending(w => w.YearFrom)
                .Select(w => new WorkExperienceItem
                {
                    CompanyName = w.CompanyName,
                    Position = w.Position,
                    YearFrom = w.YearFrom,
                    YearTo = w.YearTo,
                    Responsibilities = w.Responsibilities
                }).ToList(),
            Educations = resume.Educations
                .OrderBy(e => e.Kind)
                .Select(e => new EducationItem
                {
                    Kind = e.Kind,
                    Institution = e.Institution,
                    Specialty = e.Specialty,
                    YearFrom = e.YearFrom,
                    YearTo = e.YearTo
                }).ToList()
        };
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResumeFormViewModel model)
    {
        var userId = _users.GetUserId(User)!;
        var resume = await _db.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == userId);
        if (resume == null)
        {
            return NotFound();
        }

        model.Id = id;
        await ValidateCategory(model);
        if (!ModelState.IsValid)
        {
            await FillCategories(model);
            return View("Form", model);
        }

        Map(model, resume);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Резюме сохранено";
        return RedirectToAction(nameof(Details), new { id = resume.Id });
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(int id)
    {
        var userId = _users.GetUserId(User)!;
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == userId);
        if (resume == null)
        {
            return NotFound();
        }

        resume.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Дата обновления резюме изменена";
        return RedirectToAction("Index", "Seeker");
    }

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, string? returnTo)
    {
        var userId = _users.GetUserId(User)!;
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == userId);
        if (resume == null)
        {
            return NotFound();
        }

        resume.IsPublished = !resume.IsPublished;
        await _db.SaveChangesAsync();
        TempData["Success"] = resume.IsPublished ? "Резюме опубликовано" : "Резюме снято с публикации";

        if (returnTo == "details")
        {
            return RedirectToAction(nameof(Details), new { id });
        }
        return RedirectToAction("Index", "Seeker");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var resume = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.Seeker)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (resume == null)
        {
            return NotFound();
        }

        var isOwner = resume.SeekerId == _users.GetUserId(User);
        var employerCanSee = User.IsInRole(Roles.Employer) && resume.IsPublished;
        if (!isOwner && !employerCanSee)
        {
            return Forbid();
        }

        ViewBag.IsOwner = isOwner;
        if (User.IsInRole(Roles.Employer))
        {
            var employerId = _users.GetUserId(User)!;
            ViewBag.EmployerVacancies = await _db.Vacancies.Where(v => v.EmployerId == employerId && v.IsPublished)
                .OrderByDescending(v => v.UpdatedAt).ToListAsync();
        }
        return View(resume);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Catalog(int? categoryId, int page = 1)
    {
        var query = _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.Seeker)
            .Where(r => r.IsPublished);

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var model = new ResumeCatalogViewModel
        {
            Items = items,
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync(),
            CategoryId = categoryId,
            Page = page,
            TotalPages = totalPages
        };
        return View(model);
    }

    private async Task FillCategories(ResumeFormViewModel model)
    {
        model.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }

    private async Task ValidateCategory(ResumeFormViewModel model)
    {
        if (model.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == model.CategoryId.Value))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Выбранная категория не существует");
        }
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void Map(ResumeFormViewModel model, Resume resume)
    {
        resume.Title = model.Title.Trim();
        resume.CategoryId = model.CategoryId!.Value;
        resume.ExpectedSalary = model.ExpectedSalary;
        resume.Telegram = Clean(model.Telegram);
        resume.ContactEmail = Clean(model.ContactEmail);
        resume.Phone = Clean(model.Phone);
        resume.Facebook = Clean(model.Facebook);
        resume.LinkedIn = Clean(model.LinkedIn);
        resume.UpdatedAt = DateTime.UtcNow;

        resume.WorkExperiences.Clear();
        foreach (var item in model.WorkExperiences)
        {
            resume.WorkExperiences.Add(new WorkExperience
            {
                CompanyName = item.CompanyName.Trim(),
                Position = item.Position.Trim(),
                YearFrom = item.YearFrom,
                YearTo = item.YearTo,
                Responsibilities = Clean(item.Responsibilities)
            });
        }

        resume.Educations.Clear();
        foreach (var item in model.Educations)
        {
            resume.Educations.Add(new Education
            {
                Kind = item.Kind,
                Institution = item.Institution.Trim(),
                Specialty = Clean(item.Specialty),
                YearFrom = item.YearFrom,
                YearTo = item.YearTo
            });
        }
    }
}
