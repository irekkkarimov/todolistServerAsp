namespace todolistServer.DTOs;

public class TaskDto
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Category { get; set; }
    public DateTime Deadline { get; set; }
}