using HrCrm.Application.Services;
using HrCrm.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrCrm.WebApi.Controllers;

[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ApiBaseController
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto request)
    {
        var result = await authService.RegisterAsync(request.Username, request.Password);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
    {
        var result = await authService.LoginAsync(request.Username, request.Password);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return Ok(ToDto(result.Value!));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return Ok(ToDto(result.Value!));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult GetMe()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        return Ok(new
        {
            Id = int.TryParse(id, out var parsedId) ? parsedId : 0,
            Username = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    private static AuthResponseDto ToDto(HrCrm.Application.Common.AuthResult result) =>
        new() { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken };
}
