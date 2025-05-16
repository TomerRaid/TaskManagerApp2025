using System.ComponentModel.DataAnnotations;

namespace ProjectManagmentApp.Models
{
    public class ProjectDto
    {
        public int? Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
