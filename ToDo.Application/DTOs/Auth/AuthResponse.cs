using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public int ExpiresInMinutes { get; set; }
    public string Role { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
}
