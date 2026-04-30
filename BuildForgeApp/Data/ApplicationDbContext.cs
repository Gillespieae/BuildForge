using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Models;

namespace BuildForgeApp.Data
{
    // inherits IdentityDbContext so ASP.NET Identity tables (users, roles, etc.) are included
    public class ApplicationDbContext : IdentityDbContext
    {
        // constructor passes DB options (connection string, etc.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // tables in your database
        public DbSet<PcComponent> PcComponents { get; set; } = default!;
        public DbSet<Build> Builds { get; set; } = default!;
        public DbSet<BuildComponent> BuildComponents { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // required for Identity to work

            // Build -> User (each build belongs to one user)
            builder.Entity<Build>()
                .HasOne(b => b.User)
                .WithMany() // user can have many builds (not explicitly defined in User model)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade); // deleting user deletes their builds

            // BuildComponent -> Build (many-to-one)
            builder.Entity<BuildComponent>()
                .HasOne(bc => bc.Build)
                .WithMany(b => b.BuildComponents)
                .HasForeignKey(bc => bc.BuildId)
                .OnDelete(DeleteBehavior.Cascade); // deleting build deletes its components

            // BuildComponent -> PcComponent (many-to-one)
            builder.Entity<BuildComponent>()
                .HasOne(bc => bc.PcComponent)
                .WithMany(pc => pc.BuildComponents)
                .HasForeignKey(bc => bc.PcComponentId)
                .OnDelete(DeleteBehavior.Cascade); // deleting component removes it from builds
        }
    }
}