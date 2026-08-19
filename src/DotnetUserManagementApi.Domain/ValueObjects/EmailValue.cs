using System.Text.RegularExpressions;
using DotnetUserManagementApi.Domain.Exceptions;

namespace DotnetUserManagementApi.Domain.ValueObjects;

public sealed record EmailValue
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    public string Value { get; }

    private EmailValue(string value) => Value = value;

    public static EmailValue Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254 || !Pattern.IsMatch(email))
        {
            throw new DomainValidationException("E-mail inválido.");
        }

        return new EmailValue(email.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}