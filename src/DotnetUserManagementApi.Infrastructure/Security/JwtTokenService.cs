using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetUserManagementApi.Application.Abstractions;
using DotnetUserManagementApi.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotnetUserManagementApi.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly JwtOptions _options = options.Value;

    public TokenResult CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
            ],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(CreateSigningKey(), SecurityAlgorithms.HmacSha256));

        var value = TokenHandler.WriteToken(token);

        return new TokenResult(value, (long)(expiresAt - DateTime.UtcNow).TotalSeconds);
    }

    public SymmetricSecurityKey CreateSigningKey()
        => new(Encoding.UTF8.GetBytes(_options.Key));
}