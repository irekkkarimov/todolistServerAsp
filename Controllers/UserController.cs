using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using todolistServer.DTOs;
using todolistServer.Models;
using todolistServer.Services;
using Task = System.Threading.Tasks.Task;

namespace todolistServer.Controllers;

[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtHandlerService _jwtHandlerService;

    public UserController(IConfiguration configuration,
        ApplicationDbContext context, 
        IMapper mapper,
        IJwtHandlerService jwtHandlerService)
    {
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
        _jwtHandlerService = jwtHandlerService;
    }

    [Route("Registration")]
    [HttpPost]
    public async Task<IActionResult> Registration(UserDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        _context.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtHandlerService.CreateToken(user);
        return Ok ( new { token });
    }

    [Route("Login")]
    [HttpPost]
    public ActionResult Login([FromBody] UserLoginDto userLoginDto)
    {
        var users = _context.Users;
        var userFind = users.Any()
            ? users.FirstOrDefault(i => i.Email == userLoginDto.Email) 
            : null;
        if (userFind == null)
            return Unauthorized("User not found.");
        if (!userLoginDto.Password.Equals(userFind.Password)) 
            return Unauthorized("Wrong password");
        var token = _jwtHandlerService.CreateToken(userFind);
        return Ok( new { token , success = true });

    }

    [Authorize]
    [Route("Auth")]
    [HttpGet]
    public IActionResult Check()
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        return Ok (new { Token = token });
    }

    [Authorize]
    [Route("Edit")]
    [HttpPut]
    public async Task<ActionResult<string>> Edit([FromBody] UserEditDto userEditDto)
    {
        // Fetching email of current user
        var fetchedEmail = _jwtHandlerService.FetchUserEmail();
        if (!fetchedEmail.Item1)
            Forbid();
        
        // Fetching current user from database
        var userFromDb = await _context.Users
            .FirstOrDefaultAsync(i => i.Email == fetchedEmail.Item2);

        if (userFromDb != null)
        {
            // Replacing props values of userFromDb to update and keep
            // the previous values of other props, cause userEditDto have empty props other than 'name' and 'password'
            if (userEditDto.Name != "")
                userFromDb.Name = userEditDto.Name;
            
            if (userEditDto.Password != "")
                userFromDb.Password = userEditDto.Password;

            _context.Users.Update(userFromDb);
            await _context.SaveChangesAsync();
            return Ok("Successfully updated user");
        }

        return BadRequest("Error");
    }
}