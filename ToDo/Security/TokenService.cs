using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    public RefreshToken CreateRefreshToken(Guid userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64); // 512-bit
        var token = Convert.ToBase64String(tokenBytes);

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays)
        };
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _opt.Issuer,

            ValidateAudience = true,
            ValidAudience = _opt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key)),

            // 🔥 önemli: expire kontrolünü kapatıyoruz (refresh için)
            ValidateLifetime = false,

            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
