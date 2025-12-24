using System.Security.Claims;
using ToDo.Domain.Entities;

namespace ToDo.API.Security;

public interface ITokenService
{
    string CreateAccessToken(User user, string roleName);
    RefreshToken CreateRefreshToken(Guid userId);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
