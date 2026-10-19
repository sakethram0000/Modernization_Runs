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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(user => user.FullName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Entity<Checklist>(checklist =>
        {
            checklist.ToTable("Checklists");
            checklist.Property(item => item.Name).HasMaxLength(255).IsRequired();
            checklist.Property(item => item.AuthorId).HasMaxLength(128).IsRequired();
            checklist.Property(item => item.DateAdded).IsRequired();
        });
    }
}
