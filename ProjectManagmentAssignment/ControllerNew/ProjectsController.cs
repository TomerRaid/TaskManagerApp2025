using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentApp.Middleware;
using ProjectManagmentApp.Models;
using ProjectManagmentApp.Repositories;
using System;

namespace ProjectManagmentApp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectsController> _logger;
        private readonly IUserAccessor _userAccessor;

        public ProjectsController(
            IProjectRepository projectRepository,
            ILogger<ProjectsController> logger,
            IUserAccessor userAccessor)
        {
            _projectRepository = projectRepository;
            _logger = logger;
            _userAccessor = userAccessor;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Project>>> GetProjects([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting all projects with pagination");
                var projects = await _projectRepository.GetProjectsAsync(paginationParams);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                return StatusCode(500, "An error occurred while retrieving projects");
            }
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            try
            {
                _logger.LogInformation($"Getting project with ID {id}");
                var project = await _projectRepository.GetProjectByIdAsync(id);

                if (project == null)
                {
                    _logger.LogWarning($"Project with ID {id} not found");
                    return NotFound($"Project with ID {id} not found");
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting project with ID {id}");
                return StatusCode(500, $"An error occurred while retrieving project with ID {id}");
            }
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject([FromBody] ProjectDto projectDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for project creation");
                    return BadRequest(ModelState);
                }

                var project = new Project
                {
                    Name = projectDto.Name,
                    Description = projectDto.Description
                };

                _logger.LogInformation("Creating new project");
                var createdProject = await _projectRepository.CreateProjectAsync(project);

                return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, "An error occurred while creating the project");
            }
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto projectDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for project update");
                    return BadRequest(ModelState);
                }

                var existingProject = await _projectRepository.GetProjectByIdAsync(id);
                if (existingProject == null)
                {
                    _logger.LogWarning($"Project with ID {id} not found for update");
                    return NotFound($"Project with ID {id} not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(projectDto.Name))
                    existingProject.Name = projectDto.Name;

                if (projectDto.Description != null)
                    existingProject.Description = projectDto.Description;

                _logger.LogInformation($"Updating project with ID {id}");
                await _projectRepository.UpdateProjectAsync(existingProject);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project with ID {id}");
                return StatusCode(500, $"An error occurred while updating the project with ID {id}");
            }
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                if (!_userAccessor.IsInRoleAdmin())
                {
                    _logger.LogError($"Error deleting project with ID {id}, ONLY ADMIN MAY DELETE PROJECTS!");
                    return StatusCode(500, $"An error occurred while deleting the project with ID {id}");
                }

                _logger.LogInformation($"Deleting project with ID {id}");
                var result = await _projectRepository.DeleteProjectAsync(id);

                if (!result)
                {
                    _logger.LogWarning($"Project with ID {id} not found for deletion");
                    return NotFound($"Project with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project with ID {id}");
                return StatusCode(500, $"An error occurred while deleting the project with ID {id}");
            }
        }
    }
}
