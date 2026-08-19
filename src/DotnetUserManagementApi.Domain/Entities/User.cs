using DotnetUserManagementApi.Domain.Exceptions;
using DotnetUserManagementApi.Domain.ValueObjects;

namespace DotnetUserManagementApi.Domain.Entities;

public sealed class User
{
    public const int MaxNameLength = 100;

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
    }

    public User(string name, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        SetName(name);
        Email = EmailValue.Create(email).Value;
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
        {
            throw new DomainValidationException($"Nome é obrigatório e deve ter no máximo {MaxNameLength} caracteres.");
        }

        Name = name.Trim();
    }
}