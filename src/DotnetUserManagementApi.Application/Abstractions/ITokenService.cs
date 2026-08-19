using DotnetUserManagementApi.Domain.Entities;

namespace DotnetUserManagementApi.Application.Abstractions;

public interface ITokenService
{
    TokenResult CreateToken(User user);
}

public sealed record TokenResult(string Value, long ExpiresInSeconds);