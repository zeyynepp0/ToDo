using Microsoft.AspNetCore.Mvc;
using ToDo.API.Services;
using ToDo.Application.DTOs.Auth;
using ToDo.Application.Services;

namespace ToDo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        try { return Ok(await _auth.RegisterAsync(req)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        try { return Ok(await _auth.LoginAsync(req)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }
}
