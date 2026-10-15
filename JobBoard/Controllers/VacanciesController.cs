using JobBoard.Data;
using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobBoard.Controllers;

[Authorize]
public class VacanciesController : Controller
{
    private const int PageSize = 20;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public VacanciesController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index(string? q, int? categoryId, string sort = "new", int page = 1)
    {
        var model = await BuildListModel(q, categoryId, sort, page);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? q, int? categoryId, string sort = "new", int page = 1)
    {
        var model = await BuildListModel(q, categoryId, sort, page);
        return PartialView("_VacancyResults", model);
    }

    private async Task<VacancyListViewModel> BuildListModel(string? q, int? categoryId, string sort, int page)
    {
        var query = _db.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Employer)
            .Where(v => v.IsPublished);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(v => v.Title.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(v => v.CategoryId == categoryId.Value);
        }

        query = sort switch
        {
            "salary_desc" => query.OrderByDescending(v => v.Salary.HasValue).ThenByDescending(v => v.Salary).ThenByDescending(v => v.UpdatedAt),
            "salary_asc" => query.OrderBy(v => v.Salary).ThenByDescending(v => v.UpdatedAt),
            _ => query.OrderByDescending(v => v.UpdatedAt)
        };

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var model = new VacancyListViewModel
        {
            Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync(),
            Query = q,
            CategoryId = categoryId,
            Sort = sort,
            Page = page,
            TotalPages = totalPages
        };
        return model;
    }

    public async Task<IActionResult> Details(int id)
    {
        var vacancy = await _db.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Employer)
            .FirstOrDefaultAsync(v => v.Id == id);
        var userId = _users.GetUserId(User);
        if (vacancy == null || (!vacancy.IsPublished && vacancy.EmployerId != userId))
        {
            return NotFound();
        }

        var model = new VacancyDetailsViewModel { Vacancy = vacancy };
        ViewBag.IsOwner = vacancy.EmployerId == userId;
        if (User.IsInRole(Roles.Seeker))
        {
            model.MyResumes = await _db.Resumes
                .Where(r => r.SeekerId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
            model.ExistingApplicationId = await _db.JobApplications
                .Where(a => a.VacancyId == id && a.SeekerId == userId)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();
        }
        return View(model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new VacancyFormViewModel();
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VacancyFormViewModel model)
    {
        model.Id = null;
        await ValidateCategory(model);
        ValidateExperience(model);
        if (!ModelState.IsValid)
        {
            await FillCategories(model);
            return View("Form", model);
        }

        var vacancy = new Vacancy
        {
            EmployerId = _users.GetUserId(User)!,  
            CreatedAt = DateTime.UtcNow
        };
        Map(model, vacancy);
        _db.Vacancies.Add(vacancy);
        await _db.SaveChangesAsync();

        TempData["Success"] = "???????? ???????";
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null)
        {
            return NotFound();
        }

        var model = new VacancyFormViewModel
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Salary = vacancy.Salary,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            ExperienceFrom = vacancy.ExperienceFrom,
            ExperienceTo = vacancy.ExperienceTo,
            CategoryId = vacancy.CategoryId
        };
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VacancyFormViewModel model)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null)
        {
            return NotFound();
        }

        model.Id = id;
        await ValidateCategory(model);
        ValidateExperience(model);
        if (!ModelState.IsValid)
        {
            await FillCategories(model);
            return View("Form", model);
        }

        Map(model, vacancy);
        await _db.SaveChangesAsync();

        TempData["Success"] = "???????? ?????????";
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(int id)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null)
        {
            return NotFound();
        }
        vacancy.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, string? returnUrl)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null)
        {
            return NotFound();
        }

        vacancy.IsPublished = !vacancy.IsPublished;
        await _db.SaveChangesAsync();
        TempData["Success"] = vacancy.IsPublished ? "???????? ????????????" : "???????? ????? ? ??????????";

        if (returnUrl == "details")
        {
            return RedirectToAction(nameof(Details), new { id });
        }
        return RedirectToAction("Index", "Employer");
    }

    private async Task<Vacancy?> FindOwnVacancy(int id)
    {
        var employerId = _users.GetUserId(User)!;
        return await _db.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.EmployerId == employerId);
    }

    private async Task FillCategories(VacancyFormViewModel model)
    {
        model.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }

    private async Task ValidateCategory(VacancyFormViewModel model)
    {
        if (model.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == model.CategoryId.Value))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "????????? ????????? ?? ??????????");
        }
    }

    private static void Map(VacancyFormViewModel model, Vacancy vacancy)
    {
        vacancy.Title = model.Title.Trim();
        vacancy.Salary = model.Salary;
        vacancy.Description = model.Description.Trim();
        vacancy.Requirements = string.IsNullOrWhiteSpace(model.Requirements) ? null : model.Requirements.Trim();
        vacancy.ExperienceFrom = model.ExperienceFrom;
        vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.CategoryId = model.CategoryId!.Value;
        vacancy.UpdatedAt = DateTime.UtcNow;
    }
    private void ValidateExperience(VacancyFormViewModel model)
    {
        if (model.ExperienceFrom > model.ExperienceTo)
        {
            ModelState.AddModelError(nameof(model.ExperienceFrom),
                "??????????? ???? ?? ????? ???? ?????? ?????????????");
        }
    }
}
