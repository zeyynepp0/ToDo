using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ToDo.Domain.Entities;

namespace ToDo.API.Security;

public class TokenService: ITokenService
{
    private readonly JwtOptions _opt;

    public TokenService(IOptions<JwtOptions> options)
    {
        _opt = options.Value;
    }

    public string CreateAccessToken(User user, string roleName)
    {

        if (string.IsNullOrWhiteSpace(_opt.Key))
            throw new InvalidOperationException("JWT Key is missing. Check appsettings.json -> Jwt:Key");
        if (string.IsNullOrWhiteSpace(_opt.Issuer))
            throw new InvalidOperationException("JWT Issuer is missing. Check appsettings.json -> Jwt:Issuer");
        if (string.IsNullOrWhiteSpace(_opt.Audience))
            throw new InvalidOperationException("JWT Audience is missing. Check appsettings.json -> Jwt:Audience");

        roleName = string.IsNullOrWhiteSpace(roleName) ? "User" : roleName;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, roleName),
            new("role", roleName) // bazen frontend kolay okur
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
