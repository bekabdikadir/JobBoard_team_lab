using JobBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Resume>(e =>
        {
            e.Property(r => r.ExpectedSalary).HasPrecision(18, 2);
            e.HasOne(r => r.Seeker).WithMany().HasForeignKey(r => r.SeekerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Category).WithMany().HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => new { r.IsPublished, r.CategoryId });
        });

        builder.Entity<WorkExperience>()
            .HasOne(w => w.Resume).WithMany(r => r.WorkExperiences)
            .HasForeignKey(w => w.ResumeId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Education>()
            .HasOne(x => x.Resume).WithMany(r => r.Educations)
            .HasForeignKey(x => x.ResumeId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vacancy>(e =>
        {
            e.Property(v => v.Salary).HasPrecision(18, 2);
            e.HasOne(v => v.Employer).WithMany().HasForeignKey(v => v.EmployerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Category).WithMany().HasForeignKey(v => v.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => new { v.IsPublished, v.UpdatedAt });
        });

        builder.Entity<JobApplication>(e =>
        {
            e.HasOne(a => a.Vacancy).WithMany().HasForeignKey(a => a.VacancyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Resume).WithMany().HasForeignKey(a => a.ResumeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Seeker).WithMany().HasForeignKey(a => a.SeekerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => new { a.VacancyId, a.SeekerId }).IsUnique();
        });

        builder.Entity<ChatMessage>(e =>
        {
            e.HasOne(m => m.JobApplication).WithMany(a => a.Messages)
                .HasForeignKey(m => m.JobApplicationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(m => new { m.JobApplicationId, m.SentAt });
        });
    }
}
