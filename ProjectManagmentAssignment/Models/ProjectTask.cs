namespace ProjectManagmentApp.Models
{
    public class ProjectTask
    {

        //Each task should have a title, description, and status (e.g., todo, in-progress, done).

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int StatusId { get; set; }
        public TaskStatus Status { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TaskStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g., "Todo", "In Progress", "Done"
    }
}
