using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using todolistServer.Models;

namespace todolistServer.Services;

public class JwtHandlerService : IJwtHandlerService
{
    private readonly HttpContext _httpContext;
    private readonly IConfiguration _configuration;

    public JwtHandlerService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _configuration = configuration;
    }

    /// <summary>
    /// Fetches user id value from jwt token stored in request's
    /// "Authorization" header
    /// </summary>
    /// <returns>(bool, int) tuple, where bool says if fetching
    /// was successful and int is value if succeeded</returns>
    public (bool, int) FetchUserId()
    {
        // If I fetch claims from HttpContext.User, it transforms my custom claims into its in-build claims, wtf??
        
        // Fetching jwt token from header "Authorization"
        var token = _httpContext.Request.Headers.Authorization
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

    /// <summary>
    /// Fetches user email value from jwt token stored in request's
    /// "Authorization" header
    /// </summary>
    /// <returns>(bool, string) tuple, where bool says if fetching
    /// was successful and string is value if succeeded</returns>
    public (bool, string) FetchUserEmail()
    {
        // If I fetch claims from HttpContext.User, it transforms my custom claims into its in-build claims, wtf??
        
        // Fetching jwt token from header "Authorization"
        var token = _httpContext.Request.Headers.Authorization
            .ToString().Split(" ")[1];

        // Making new instance of JwtSecurityToken with current token
        // and getting all claims of jwt
        var jwtSecurityToken = new JwtSecurityToken(jwtEncodedString: token);
        var decoded = jwtSecurityToken.Claims
            .Select(i => new { i.Type, i.Value })
            .ToList();

        // If jwt decoded doesn't contain claim "userid", then return false
        if (decoded.All(i => i.Type != "email"))
            return (false, "");

        // If it contains claim "userid", then setting its value to userid variable and returning it
        var userEmail = decoded.First(i => i.Type == "email").Value;
        return (true, userEmail);
    }
    
    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("userid", user.UserId.ToString()),
            new Claim("name", user.Name),
            new Claim("email", user.Email),
            new Claim("url", user.PictureUrl ?? "")
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