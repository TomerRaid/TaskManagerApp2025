using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Data;
using ProjectManagmentApp.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ProjectManagmentApp.Models
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Project>> GetProjectsAsync(PaginationParams pagingParams)
        {
            var query = _context.Projects.AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParams.PageNumber - 1) * pagingParams.PageSize)
                .Take(pagingParams.PageSize)
                .ToListAsync();

            return new PaginatedResult<Project>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pagingParams.PageNumber,
                PageSize = pagingParams.PageSize
            };
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            return project ?? throw new InvalidOperationException($"Project with ID {id} not found.");
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
