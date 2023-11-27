using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace todolistServer.Models;

public class Task
{
    [Key] public int TaskId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; } = new();
    
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public int Category { get; set; }
    
    [Column(TypeName = "TIMESTAMP")]
    public DateTime Deadline { get; set; }
}