using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using todolistServer.DTOs;
using todolistServer.Models;
using Task = System.Threading.Tasks.Task;

namespace todolistServer.Controllers;

[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserController(IConfiguration configuration, ApplicationDbContext context, IMapper mapper)
    {
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
    }

    [Route("Registration")]
    [HttpPost]
    public async Task<string> Registration(UserDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        _context.Add(user);
        await _context.SaveChangesAsync();

        var token = CreateToken(user);
        return token;
    }

    [Route("Login")]
    [HttpPost]
    public ActionResult<string> Login(UserLoginDto userLoginDto)
    {
        var users = _context.Users;
        var userFind = users.Any()
            ? users.First(i => i.Email == userLoginDto.Email) 
            : null;
        if (userFind == null)
            return BadRequest("User not found.");
        if (!userLoginDto.Password.Equals(userFind.Password)) 
            return BadRequest("Wrong password");
        var token = CreateToken(userFind);
        return token;

    }

    [Authorize]
    [Route("Auth")]
    [HttpGet]
    public ActionResult<JsonTokenDto> Check()
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        return new JsonTokenDto { Token = token };
    }

    [Authorize]
    [Route("Edit")]
    [HttpPut]
    public async Task<ActionResult<string>> Edit([FromBody] UserEditDto userEditDto)
    {
        // Allowed updating 'Name', 'Password
        var jwtEmail = HttpContext.User.Claims
            .Single(i => i.Type == "Email").Value;
        var userFromDb = _context.Users
            .First(i => i.Email == jwtEmail);

        if (userFromDb != null)
        {
            // Fetching current use from database and replacing props to update to keep
            // the previous values of other props, cause userToUpdate have empty props after mapping
            var userToUpdate = _mapper.Map<User>(userEditDto);
            userFromDb.Name = userToUpdate.Name;
            if (userToUpdate.Password != "")
                userFromDb.Password = userToUpdate.Password;

            _context.Users.Update(userFromDb);
            await _context.SaveChangesAsync();
            return Ok("Successfully updated user");
        }

        return BadRequest("Error");
    }
    
    [NonAction]
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("userid", user.UserId.ToString()),
            new Claim("name", user.Name),
            new Claim("email", user.Email)
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
       
        var creds = new SigningCredentials(key, SecurityAlgorithms.Aes128CbcHmacSha256);
       
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
            );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}