using System.ComponentModel.DataAnnotations;
using ProjectManagmentApp.Models;

namespace ProjectManagmentApp.Repositories
{
    public interface IProjectRepository
    {
        Task<PaginatedResult<Project>> GetProjectsAsync(PaginationParams pagingParams);
        Task<Project> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
    }
}
