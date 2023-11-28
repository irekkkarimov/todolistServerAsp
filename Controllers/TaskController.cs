using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todolistServer.DTOs;
using todolistServer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using todolistServer.Services;
using Task = todolistServer.Models.Task;

namespace todolistServer.Controllers;

[EnableCors]
[ApiController]
[Route("api/Task")]
public class TaskController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtHandlerService _jwtHandlerService;

    public TaskController(ApplicationDbContext context, IMapper mapper, IJwtHandlerService jwtHandlerService)
    {
        _context = context;
        _mapper = mapper;
        _jwtHandlerService = jwtHandlerService;
    }

    [Authorize]
    [HttpGet]
    [Route("GetAll")]
    public IActionResult GetAll()
    {
        var fetchedUserId = _jwtHandlerService.FetchUserId();
        if (fetchedUserId.Item1)
            Forbid();
        
        var tasksDto = _context.Tasks
            .Include(i => i.User)
            .Where(i => i.User.UserId == fetchedUserId.Item2)
            .Select(i => _mapper.Map<TaskDto>(i))
            .ToList();

        return Ok(new { tasks = tasksDto });
    }

    [Authorize]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(
        [FromBody] TaskCreateDto taskCreateDto)
    {
        var fetchedUserId = _jwtHandlerService.FetchUserId();
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
        var fetchedUserId = _jwtHandlerService.FetchUserId();
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
}