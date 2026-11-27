using Capstone.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Capstone.DAL.Repository;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
        : base(options)
    {
    }

    public class HelpDeskDbContextFactory : IDesignTimeDbContextFactory<HelpDeskDbContext>
    {
        public HelpDeskDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
                .UseSqlite(@"Data Source=Database\ServiceDeskDB.db")
                .Options;

            return new HelpDeskDbContext(options);
        }
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<Status> Statuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Status>().ToTable("Status");
        modelBuilder.Entity<ServiceRequest>().ToTable("ServiceRequest");

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.RaisedByUser)
            .WithMany()
            .HasForeignKey(sr => sr.RaisedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Status)
            .WithMany()
            .HasForeignKey(sr => sr.ReqStatus)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "User" }
        );

        modelBuilder.Entity<Status>().HasData(
            new Status { StatusId = 1, Description = "Open" },
            new Status { StatusId = 2, Description = "In Progress" },
            new Status { StatusId = 3, Description = "Closed" },
            new Status { StatusId = 4, Description = "Re-Opened" }
        );
    }
}
