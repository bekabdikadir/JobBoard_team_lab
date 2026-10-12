using JobBoard.Data;
using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Controllers;

[Authorize(Roles = Roles.Employer)]
public class ProfilesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ProfilesController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Seeker(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null || !await _users.IsInRoleAsync(user, Roles.Seeker))
        {
            return NotFound();
        }

        var model = new SeekerProfileViewModel
        {
            User = user,
            Resumes = await _db.Resumes
                .Include(r => r.Category)
                .Where(r => r.SeekerId == id && r.IsPublished)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync()
        };
        return View(model);
    }
}
