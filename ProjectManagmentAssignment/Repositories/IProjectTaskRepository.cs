using System.ComponentModel.DataAnnotations;
using ProjectManagmentApp.DTO;
using ProjectManagmentApp.Models;

namespace ProjectManagmentApp.Repositories
{
    public interface IProjectTaskRepository
    {
        Task<PaginatedResult<TaskDto>> GetTasksAsync(int projectId, PaginationParams pagingParams);
        Task<ProjectTask> GetTaskByIdAsync(int id);
        Task<ProjectTask> CreateTaskAsync(ProjectTask task);
        Task<ProjectTask> UpdateTaskAsync(ProjectTask task);
        Task<bool> DeleteTaskAsync(int id);
    }
}
