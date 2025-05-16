using System.ComponentModel.DataAnnotations;

namespace ProjectManagmentApp.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // Navigation property
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        // Audit fields
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
