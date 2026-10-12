using JobBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(Roles.Employer))
            {
                return RedirectToAction("Catalog", "Resumes");
            }
            return RedirectToAction("Index", "Seeker");
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
