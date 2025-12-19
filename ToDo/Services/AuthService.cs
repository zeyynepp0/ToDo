
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ToDo.API.Security;
using ToDo.Application.DTOs.Auth;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Contexts;

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
        await _db.SaveChangesAsync();

        var token = _tokens.CreateAccessToken(user, userRole.Name);

        return new AuthResponse
        {
            AccessToken = token,
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

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresInMinutes = _jwt.AccessTokenMinutes,
            Role = roleName,
            UserId = user.Id.ToString(),
            Email = user.Email
        };
    }
}
