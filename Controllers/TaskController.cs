using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todolistServer.DTOs;
using todolistServer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Task = todolistServer.Models.Task;

namespace todolistServer.Controllers;

[EnableCors]
[ApiController]
[Route("api/Task")]
public class TaskController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TaskController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("GetAll")]
    public IActionResult GetAll([FromQuery(Name = "userId")] int userId)
    {
        var tasksDto = _context.Tasks
            .Include(i => i.User)
            .Where(i => i.User.UserId == userId)
            .Select(i => _mapper.Map<TaskDto>(i))
            .ToList();

        return Ok(new { tasks = tasksDto });
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(
        [FromBody] TaskCreateDto taskCreateDto)
    {
        var fetchedUserId = FetchUserId();
        if (fetchedUserId.Item1)
            Forbid();

        var user = await _context.Users.FindAsync(fetchedUserId.Item2);
        var task = _mapper.Map<Task>(taskCreateDto);
        task.User = user;
        _context.Add(task);
        await _context.SaveChangesAsync();
        return Ok(taskCreateDto);
    }

    [Authorize]
    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete([FromQuery(Name = "taskId")] int taskId)
    {
        var fetchedUserId = FetchUserId();
        if (!fetchedUserId.Item1)
            return Forbid();

        var task = await _context.Tasks.FindAsync(taskId);

        // If task was not found returning 404
        if (task == null)
            return BadRequest(new { message = "Task not found" });

        // If task was found but it doesn't belong to current user, then returning 403
        if (task.UserId != fetchedUserId.Item2)
            return Forbid();

        // If everything is okay deleting the task and returning 200
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Successfully deleted" });

    }

    [NonAction]
    private (bool, int) FetchUserId()
    {
        // Fetching jwt token from header "Authorization"
        var token = HttpContext.Request.Headers.Authorization
            .ToString().Split(" ")[1];

        // Making new instance of JwtSecurityToken with current token
        // and getting all claims of jwt
        var jwtSecurityToken = new JwtSecurityToken(jwtEncodedString: token);
        var decoded = jwtSecurityToken.Claims
            .Select(i => new { i.Type, i.Value })
            .ToList();

        // If jwt decoded doesn't contain claim "userid", then return false
        if (decoded.All(i => i.Type != "userid"))
            return (false, 0);

        // If it contains claim "userid", then setting its value to userid variable and returning it
        var userId = int.Parse(decoded.First(i => i.Type == "userid").Value);
        return (true, userId);
    }
}