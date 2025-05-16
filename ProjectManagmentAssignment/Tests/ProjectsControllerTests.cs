using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagmentApp.Controllers;
using ProjectManagmentApp.Middleware;
using ProjectManagmentApp.Models;

using ProjectManagmentApp.Repositories;
using Xunit;


namespace ProjectManagmentAssignment.Tests
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ILogger<ProjectsController>> _mockLogger;
        private readonly Mock<IUserAccessor> _mockUserAccessor;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockLogger = new Mock<ILogger<ProjectsController>>();
            _mockUserAccessor = new Mock<IUserAccessor>();
            _controller = new ProjectsController(_mockProjectRepository.Object, _mockLogger.Object, _mockUserAccessor.Object);
        }

        [Fact]
        public async Task GetProjects_ReturnsOkResult_WithPaginatedProjects()
        {
            // Arrange
            var paginatedProjects = new PaginatedResult<Project>
            {
                Items = new List<Project>
                {
                    new Project { Id = 1, Name = "Project 1" },
                    new Project { Id = 2, Name = "Project 2" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockProjectRepository.Setup(repo => repo.GetProjectsAsync(It.IsAny<PaginationParams>()))
                .ReturnsAsync(paginatedProjects);

            // Act
            var result = await _controller.GetProjects(new PaginationParams());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PaginatedResult<Project>>(okResult.Value);
            Assert.Equal(2, returnValue.TotalCount);
            Assert.Equal(2, returnValue.Items.Count());
        }

        [Fact]
        public async Task GetProject_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description"
            };

            _mockProjectRepository.Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Project>(okResult.Value);
            Assert.Equal(projectId, returnValue.Id);
            Assert.Equal("Test Project", returnValue.Name);
        }

        [Fact]
        public async Task GetProject_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var projectId = 999;
            _mockProjectRepository.Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateProject_WithValidModel_ReturnsCreatedAtAction()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                Name = "New Project",
                Description = "New Description"
            };

            var createdProject = new Project
            {
                Id = 1,
                Name = projectDto.Name,
                Description = projectDto.Description
            };

            _mockProjectRepository.Setup(repo => repo.CreateProjectAsync(It.IsAny<Project>()))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(projectDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Project>(createdAtActionResult.Value);
            Assert.Equal(1, returnValue.Id);
            Assert.Equal(projectDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task UpdateProject_WithValidIdAndModel_ReturnsNoContent()
        {
            // Arrange
            var projectId = 1;
            var projectDto = new ProjectDto
            {
                Name = "Updated Project",
                Description = "Updated Description"
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = "Original Project",
                Description = "Original Description"
            };

            _mockProjectRepository.Setup(repo => repo.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);

            _mockProjectRepository.Setup(repo => repo.UpdateProjectAsync(It.IsAny<Project>()))
                .ReturnsAsync(existingProject);

            // Act
            var result = await _controller.UpdateProject(projectId, projectDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

/*        [Fact]
        public async Task DeleteProject_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var projectId = 1;
            _mockProjectRepository.Setup(repo => repo.DeleteProjectAsync(projectId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProject_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var projectId = 999;
            _mockProjectRepository.Setup(repo => repo.DeleteProjectAsync(projectId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }*/ // update later later
    }
}