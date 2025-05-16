using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Models;
using ProjectManagmentApp.Data;
using ProjectManagmentApp.DTO;
using System.Threading.Tasks;

namespace ProjectManagmentApp.Repositories
{
    public class ProjectTaskRepository : IProjectTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<TaskDto>> GetTasksAsync(int projectId, PaginationParams pagingParams)
        {
            var query = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
               .Skip((pagingParams.PageNumber - 1) * pagingParams.PageSize)
               .Take(pagingParams.PageSize)
               .Select(task => new TaskDto(task)) 
               .ToListAsync();

            return new PaginatedResult<TaskDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pagingParams.PageNumber,
                PageSize = pagingParams.PageSize
            };
        }

        public async Task<ProjectTask> GetTaskByIdAsync(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task ?? throw new InvalidOperationException($"Task with ID {id} not found.");
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<ProjectTask> UpdateTaskAsync(ProjectTask task)
        {
            var existing = await _context.Tasks.FindAsync(task.Id) ?? throw new InvalidOperationException($"Task with ID {task.Id} not found.");
            existing.Status = task.Status;
            existing.StatusId = task.StatusId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Description = task.Description;
            existing.Title = task.Title;
            
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
