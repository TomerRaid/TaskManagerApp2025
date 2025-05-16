using System.Collections;
using System.ComponentModel.DataAnnotations;
using ProjectManagmentApp.Models;

namespace ProjectManagmentApp.DTO
{
    public class TaskDto
    {
        public TaskDto()
        {
            
        }
        public TaskDto(ProjectTask task)
        {
             Id = task.Id;
            StatusId = task.StatusId;
            //Status = task.Status.Name;
            Title = task.Title;
            Description = task.Description;
            ProjectId = task.ProjectId;
        }

        public int? Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public string Status { get; set; }
        public int? StatusId { get; set; }
        public int? ProjectId { get; set; }

    }
}
