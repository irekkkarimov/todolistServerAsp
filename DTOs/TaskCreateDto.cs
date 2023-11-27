namespace todolistServer.DTOs;

public class TaskCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Category { get; set; }
    public DateTime Deadline { get; set; }
}