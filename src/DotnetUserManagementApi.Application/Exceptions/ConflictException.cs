using DotnetUserManagementApi.Domain.Exceptions;

namespace DotnetUserManagementApi.Application.Exceptions;

public sealed class ConflictException(string message) : DomainException(message);