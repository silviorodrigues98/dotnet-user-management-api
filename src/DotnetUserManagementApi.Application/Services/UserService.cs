using DotnetUserManagementApi.Application.Abstractions;
using DotnetUserManagementApi.Application.Dtos;
using DotnetUserManagementApi.Application.Exceptions;
using DotnetUserManagementApi.Domain.Entities;
using DotnetUserManagementApi.Domain.Exceptions;
using DotnetUserManagementApi.Domain.ValueObjects;

namespace DotnetUserManagementApi.Application.Services;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class UserService(IUserRepository repository, IPasswordHasher passwordHasher, ITokenService tokenService) : IUserService
{
    public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);

        var email = EmailValue.Create(request.Email);

        var existing = await repository.GetByEmailAsync(email.Value, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        var user = new User(
            request.Name,
            email.Value,
            passwordHasher.Hash(request.Password));

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = EmailValue.Create(request.Email);

        var user = await repository.GetByEmailAsync(email.Value, cancellationToken)
                   ?? throw new InvalidCredentialsException("E-mail ou senha inválidos.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("E-mail ou senha inválidos.");
        }

        var token = tokenService.CreateToken(user);

        return new LoginResponse(token.Value, "Bearer", token.ExpiresInSeconds, ToDto(user));
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return users.Select(ToDto).ToList();
    }

    private static void ValidateRegistration(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainValidationException("Nome é obrigatório.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            throw new DomainValidationException("A senha deve ter no mínimo 8 caracteres.");
        }
    }

    private static UserDto ToDto(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.CreatedAtUtc);
}