namespace DotnetUserManagementApi.Application.Dtos;

public sealed record RegisterUserRequest(string Name, string Email, string Password);