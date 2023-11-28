using todolistServer.Models;

namespace todolistServer.Services;

public interface IJwtHandlerService
{
    /// <summary>
    /// Fetches user id value from jwt token stored in request's
    /// "Authorization" header
    /// </summary>
    /// <returns>(bool, int) tuple, where bool says if fetching
    /// was successful and int is value if succeeded</returns>
    (bool, int) FetchUserId();
    
    /// <summary>
    /// Fetches user email value from jwt token stored in request's
    /// "Authorization" header
    /// </summary>
    /// <returns>(bool, string) tuple, where bool says if fetching
    /// was successful and string is value if succeeded</returns>
    (bool, string) FetchUserEmail();

    public string CreateToken(User user);
}