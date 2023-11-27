using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace todolistServer.Models;

public class User
{
    [Key] public int UserId { get; set; }

    [Column(TypeName = "VARCHAR(50)")] public string Email { get; set; }

    [Column(TypeName = "VARCHAR(50)")]
    public string Name { get; set; } = "";
    
    [Column(TypeName = "VARCHAR(50)")]
    public string Lastname { get; set; } = "";

    [Column(TypeName = "VARCHAR(50)")]
    public string Password { get; set; }
    
    public string? PictureUrl { get; set; }
}