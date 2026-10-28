using ChecklistsModern.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Checklist> Checklists => Set<Checklist>();

    public DbSet<ChecklistTask> Tasks => Set<ChecklistTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Checklist>(checklist =>
        {
            checklist.ToTable("Checklists");
            checklist.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            checklist.HasIndex(c => c.AuthorId);
        });

        builder.Entity<ChecklistTask>(task =>
        {
            task.ToTable("Tasks");
            task.HasOne(t => t.Checklist)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);
            task.HasIndex(t => t.ChecklistId);
        });
    }
}
