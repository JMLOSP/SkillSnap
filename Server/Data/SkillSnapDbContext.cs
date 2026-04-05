using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Data
{
  public class SkillSnapDbContext : IdentityDbContext<ApplicationUser>
  {
    public SkillSnapDbContext(DbContextOptions<SkillSnapDbContext> options)
      : base(options)
    {
    }

    public DbSet<PortfolioUser> PortfolioUsers => Set<PortfolioUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<PortfolioUser>()
        .HasMany(user => user.Projects)
        .WithOne(project => project.PortfolioUser)
        .HasForeignKey(project => project.PortfolioUserId)
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<PortfolioUser>()
        .HasMany(user => user.Skills)
        .WithOne(skill => skill.PortfolioUser)
        .HasForeignKey(skill => skill.PortfolioUserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}