using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Models;
using TaskStatus = ProjectManagmentApp.Models.TaskStatus;

namespace ProjectManagmentApp
{
/*    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskStatus>().HasData(
                new TaskStatus { Id = 1, Name = "To Do" },
                new TaskStatus { Id = 2, Name = "In Progress" },
                new TaskStatus { Id = 3, Name = "Done" }
            );
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectTask> Tasks { get; set; } = null!;
        public DbSet<TaskStatus> TaskStatuses { get; set; } = null!;
    }*/
}
