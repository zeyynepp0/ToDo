using ToDo.Domain.Entities;

namespace ToDo.API.Security;

public interface ITokenService
{
    string CreateAccessToken(User user, string roleName);
}
