using JobBoard.Data;
using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        return View(model);
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
}
