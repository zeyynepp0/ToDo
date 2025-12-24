
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ToDo.API.Security;
using ToDo.Application.DTOs.Auth;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Contexts;
using System.IdentityModel.Tokens.Jwt;

namespace ToDo.API.Services;

public class AuthService: IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;
    private readonly JwtOptions _jwt;

    public AuthService(AppDbContext db, ITokenService tokens, IOptions<JwtOptions> jwtOpt)
    {
        _db = db;
        _tokens = tokens;
        _jwt = jwtOpt.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email);
        if (exists) throw new InvalidOperationException("Email already exists.");

        var userRole = await _db.Roles.FirstAsync(r => r.Name == "User" && !r.IsDeleted);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Email = email,
            RoleId = userRole.Id,
            PasswordHash = PasswordHasher.Hash(req.Password), // 
            IsDeleted = false,
            RegisteredDate = DateTime.UtcNow
        };

        _db.Users.Add(user);

        var refresh = _tokens.CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refresh);

        await _db.SaveChangesAsync();

        var token = _tokens.CreateAccessToken(user, userRole.Name);

        return new AuthResponse
        {
            AccessToken = token,
            RefreshToken = refresh.Token,
            ExpiresInMinutes = _jwt.AccessTokenMinutes,
            Role = userRole.Name,
            UserId = user.Id.ToString(),
            Email = user.Email
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

        if (user is null) throw new InvalidOperationException("Invalid credentials.");

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials.");

        var roleName = user.Role?.Name ?? "User";

        var token = _tokens.CreateAccessToken(user, roleName);

        var refresh = _tokens.CreateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refresh);

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = token,
            RefreshToken = refresh.Token,
            ExpiresInMinutes = _jwt.AccessTokenMinutes,
            Role = roleName,
            UserId = user.Id.ToString(),
            Email = user.Email
        };
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest req)
    {
        var principal = _tokens.GetPrincipalFromExpiredToken(req.AccessToken);

        var userIdStr =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("Invalid token");

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == req.RefreshToken && x.UserId == userId);

        if (storedToken is null || !storedToken.IsActive)
            throw new InvalidOperationException("Invalid refresh token");

        // 🔁 ROTATION: eskiyi revoke et
        storedToken.RevokedAt = DateTime.UtcNow;

        // yeni refresh oluştur
        var newRefresh = _tokens.CreateRefreshToken(userId);

        // zincir bağlantısı (opsiyonel ama iyi)
        storedToken.ReplacedByTokenId = newRefresh.Id;

        _db.RefreshTokens.Add(newRefresh);

        // access token üret
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstAsync(x => x.Id == userId);

        var role = user.Role?.Name ?? principal.FindFirstValue(ClaimTypes.Role) ?? "User";

        var newAccess = _tokens.CreateAccessToken(user, role);

        await _db.SaveChangesAsync();

        return new TokenResponse(newAccess, newRefresh.Token);
    }

}
