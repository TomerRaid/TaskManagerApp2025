using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Models;
using ProjectManagmentApp.DTO;
using ProjectManagmentApp.Repositories;

namespace ProjectManagmentApp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects/{projectId}/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            IProjectTaskRepository taskRepository,
            IProjectRepository projectRepository,
            ILogger<TasksController> logger)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        // GET: api/projects/1/tasks
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TaskDto>>> GetTasks(int projectId, [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                // Verify project exists
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning($"Project with ID {projectId} not found");
                    return NotFound($"Project with ID {projectId} not found");
                }

                _logger.LogInformation($"Getting tasks for project {projectId} with pagination");
                var tasks = await _taskRepository.GetTasksAsync(projectId, paginationParams);
               
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tasks for project {projectId}");
                return StatusCode(500, $"An error occurred while retrieving tasks for project {projectId}");
            }
        }

        // GET: api/projects/1/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int projectId, int id)
        {
            try
            {
                _logger.LogInformation($"Getting task {id} from project {projectId}");
                var task = await _taskRepository.GetTaskByIdAsync(id);

                if (task == null)
                {
                    _logger.LogWarning($"Task with ID {id} not found");
                    return NotFound($"Task with ID {id} not found");
                }

                if (task.ProjectId != projectId)
                {
                    _logger.LogWarning($"Task with ID {id} does not belong to project {projectId}");
                    return BadRequest($"Task with ID {id} does not belong to project {projectId}");
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting task {id} from project {projectId}");
                return StatusCode(500, $"An error occurred while retrieving task {id} from project {projectId}");
            }
        }

        // POST: api/projects/1/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(int projectId, [FromBody] TaskDto taskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for task creation");
                    return BadRequest(ModelState);
                }

                // Verify project exists
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning($"Project with ID {projectId} not found");
                    return NotFound($"Project with ID {projectId} not found");
                }

                var task = new ProjectTask
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    StatusId = taskDto.StatusId ?? 1, //To do
                    //Status = new Models.TaskStatus() { Id = taskDto.Id.Value, Name = taskDto.Title },
                    ProjectId = projectId
                };

                _logger.LogInformation($"Creating new task for project {projectId}");
                var createdTask = await _taskRepository.CreateTaskAsync(task);

                return CreatedAtAction(nameof(GetTask), new { projectId, id = createdTask.Id }, createdTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating task for project {projectId}");
                return StatusCode(500, $"An error occurred while creating the task for project {projectId}");
            }
        }

        // PUT: api/projects/1/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int projectId, int id, [FromBody] TaskDto taskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for task update");
                    return BadRequest(ModelState);
                }

                var existingTask = await _taskRepository.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    _logger.LogWarning($"Task with ID {id} not found for update");
                    return NotFound($"Task with ID {id} not found");
                }

                if (existingTask.ProjectId != projectId)
                {
                    _logger.LogWarning($"Task with ID {id} does not belong to project {projectId}");
                    return BadRequest($"Task with ID {id} does not belong to project {projectId}");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(taskDto.Title))
                    existingTask.Title = taskDto.Title;

                if (taskDto.Description != null)
                    existingTask.Description = taskDto.Description;

                if (taskDto.Status is not null && taskDto.Id is not null)
                    existingTask.Status = new Models.TaskStatus() {  Id = taskDto.Id.Value , Name = taskDto.Title };

                _logger.LogInformation($"Updating task {id} in project {projectId}");
                await _taskRepository.UpdateTaskAsync(existingTask);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {id} in project {projectId}");
                return StatusCode(500, $"An error occurred while updating task {id} in project {projectId}");
            }
        }

        // DELETE: api/projects/1/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int projectId, int id)
        {
            try
            {
                var existingTask = await _taskRepository.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    _logger.LogWarning($"Task with ID {id} not found for deletion");
                    return NotFound($"Task with ID {id} not found");
                }

                if (existingTask.ProjectId != projectId)
                {
                    _logger.LogWarning($"Task with ID {id} does not belong to project {projectId}");
                    return BadRequest($"Task with ID {id} does not belong to project {projectId}");
                }

                _logger.LogInformation($"Deleting task {id} from project {projectId}");
                var result = await _taskRepository.DeleteTaskAsync(id);

                if (!result)
                {
                    return NotFound($"Task with ID {id} not found for deletion");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task {id} from project {projectId}");
                return StatusCode(500, $"An error occurred while deleting task {id} from project {projectId}");
            }
        }
    }
}
