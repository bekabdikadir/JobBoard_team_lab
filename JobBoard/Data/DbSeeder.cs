using JobBoard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { Roles.Seeker, Roles.Employer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await db.Categories.AnyAsync())
        {
            var names = new[]
            {
                "ИТ и разработка", "Маркетинг", "Продажи", "Дизайн", "Финансы и бухгалтерия",
                "Администрирование", "Образование", "Медицина", "Производство", "Логистика"
            };
            db.Categories.AddRange(names.Select(n => new Category { Name = n }));
            await db.SaveChangesAsync();
        }

        const string employerEmail = "employer@demo.local";
        var employer = await userManager.FindByEmailAsync(employerEmail);
        if (employer == null)
        {
            employer = new ApplicationUser
            {
                UserName = employerEmail,
                Email = employerEmail,
                PhoneNumber = "+996555000000",
                DisplayName = "Demo Company",
                EmailConfirmed = true
            };
            var created = await userManager.CreateAsync(employer, "Employer123");
            if (created.Succeeded)
            {
                await userManager.AddToRoleAsync(employer, Roles.Employer);
            }
        }

        if (employer.Id != null && !await db.Vacancies.AnyAsync())
        {
            var categories = await db.Categories.OrderBy(c => c.Id).Take(4).ToListAsync();
            var now = DateTime.UtcNow;
            var items = new[]
            {
                ("C# разработчик", 150000m, "Разработка веб-приложений на ASP.NET Core, PostgreSQL, Entity Framework Core.", 1, 3),
                ("Маркетолог", 90000m, "Ведение рекламных кампаний, аналитика, работа с социальными сетями.", 1, 5),
                ("Менеджер по продажам", 80000m, "Поиск клиентов, ведение переговоров, заключение сделок.", 0, 2),
                ("UI/UX дизайнер", 110000m, "Проектирование интерфейсов, создание прототипов в Figma.", 2, 4)
            };
            for (var i = 0; i < items.Length; i++)
            {
                db.Vacancies.Add(new Vacancy
                {
                    Title = items[i].Item1,
                    Salary = items[i].Item2,
                    Description = items[i].Item3,
                    ExperienceFrom = items[i].Item4,
                    ExperienceTo = items[i].Item5,
                    CategoryId = categories[i % categories.Count].Id,
                    EmployerId = employer.Id,
                    IsPublished = true,
                    CreatedAt = now.AddDays(-i),
                    UpdatedAt = now.AddDays(-i)
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
