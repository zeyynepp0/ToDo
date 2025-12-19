using ToDo.Application.DTOs.Auth;

namespace ToDo.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req);
    Task<AuthResponse> LoginAsync(LoginRequest req);
}

