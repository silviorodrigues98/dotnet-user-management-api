namespace DotnetUserManagementApi.Application.Dtos;

public sealed record LoginResponse(string Token, string TokenType, long ExpiresInSeconds, UserDto User);