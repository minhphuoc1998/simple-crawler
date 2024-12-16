using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;
    public AuthController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("login", Name = "Login")]
    public async Task<ActionResult<AuthenticatedUser>> Login([FromBody] LoginDto dto)
    {
        var existingUser = await _userService.FindOne(dto.Username);
        if (existingUser == null) {
            return NotFound();
        }
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, existingUser.Password)) {
            return Unauthorized();
        }
        var token = _authService.GenerateToken(existingUser);
        return Ok(new AuthenticatedUser{
            Username = existingUser.Username!,
            Token = token
        });
    }
}
