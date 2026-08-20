using DotnetUserManagementApi.Application.Dtos;
using DotnetUserManagementApi.Application.Services;
using DotnetUserManagementApi.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace DotnetUserManagementApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserService userService, ILoginThrottle loginThrottle) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
        => CreatedAtAction(nameof(Register), new { }, await userService.RegisterAsync(request, cancellationToken));

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var throttleKey = ClientKey();

        if (loginThrottle.IsBlocked(throttleKey))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Muitas Tentativas",
                Detail = "Muitas tentativas de login. Aguarde alguns minutos e tente novamente.",
            });
        }

        try
        {
            var result = await userService.LoginAsync(request, cancellationToken);
            loginThrottle.Reset(throttleKey);
            return Ok(result);
        }
        catch (Application.Exceptions.InvalidCredentialsException)
        {
            loginThrottle.RecordFailure(throttleKey);
            throw;
        }
    }

    private string ClientKey()
        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}