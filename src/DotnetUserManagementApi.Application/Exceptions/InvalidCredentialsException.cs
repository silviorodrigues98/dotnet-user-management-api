using DotnetUserManagementApi.Domain.Exceptions;

namespace DotnetUserManagementApi.Application.Exceptions;

public sealed class InvalidCredentialsException(string message) : DomainException(message);