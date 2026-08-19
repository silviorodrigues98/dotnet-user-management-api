namespace DotnetUserManagementApi.Application.Dtos;

public sealed record UserDto(Guid Id, string Name, string Email, DateTime CreatedAtUtc);