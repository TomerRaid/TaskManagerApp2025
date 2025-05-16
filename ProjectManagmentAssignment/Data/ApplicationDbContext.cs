using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Middleware;
using ProjectManagmentApp.Models;
using TaskStatus = ProjectManagmentApp.Models.TaskStatus;

namespace ProjectManagmentApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IUserAccessor _userAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserAccessor userAccessor)
            : base(options)
        {
            _userAccessor = userAccessor;
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<TaskStatus> TaskStatuses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskStatus>().HasData(
                new TaskStatus { Id = 1, Name = "To Do" },
                new TaskStatus { Id = 2, Name = "In Progress" },
                new TaskStatus { Id = 3, Name = "Done" }
            );
            // Project configuration
            modelBuilder.Entity<Project>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Project>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Project>()
                .Property(p => p.Description)
                .HasMaxLength(500);

            // Task configuration
            modelBuilder.Entity<ProjectTask>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Description)
                .HasMaxLength(1000);

            // Define relationships
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .Property(p => p.CreatedBy)
                .IsRequired();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.CreatedBy)
                .IsRequired();
        }

        // Override SaveChanges to implement auditing
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var username = GetCurrentUsername();
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Project project)
                    {
                        project.CreatedBy = username;
                        project.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is ProjectTask task)
                    {
                        task.CreatedBy = username;
                        task.CreatedAt = DateTime.UtcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Project project)
                    {
                        project.UpdatedBy = username;
                        project.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is ProjectTask task)
                    {
                        task.UpdatedBy = username;
                        task.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }

        private string GetCurrentUsername()
        {
            return _userAccessor.GetUsername() ?? "system";
        }
    }
}
