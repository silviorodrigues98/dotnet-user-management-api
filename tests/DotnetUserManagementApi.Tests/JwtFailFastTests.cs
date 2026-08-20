using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotnetUserManagementApi.Tests;

/// <summary>
/// D-08/T-02-01: fail-fast de Jwt:Key (JWT__KEY) — obrigatória fora de Development.
/// </summary>
public sealed class JwtFailFastTests
{
    private const string PlaceholderKey = "changeme-troque-por-uma-chave-aleatoria-de-ao-menos-32-bytes";

    [Fact]
    public void Production_MissingJwtKey_ThrowsInvalidOperationException()
    {
        using var factory = new JwtFailFastFactory(jwtKey: string.Empty, environment: "Production");

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("JWT__KEY", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_PlaceholderJwtKey_ThrowsInvalidOperationException()
    {
        // Valor de exemplo do .env.example (contém "changeme") — rejeitado em produção
        using var factory = new JwtFailFastFactory(jwtKey: PlaceholderKey, environment: "Production");

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains(".env.example", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_WeakJwtKey_ThrowsInvalidOperationException()
    {
        // Menos de 32 bytes UTF-8 — chave fraca rejeitada em produção
        using var factory = new JwtFailFastFactory(jwtKey: "short", environment: "Production");

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("32", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Development_MissingJwtKey_GeneratesKeyAndServesRequests()
    {
        // Development sem Jwt:Key gera chave aleatória e continua (comportamento atual preservado)
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class JwtFailFastFactory : WebApplicationFactory<Program>
    {
        private readonly string _jwtKey;
        private readonly string _environment;

        public JwtFailFastFactory(string jwtKey, string environment)
        {
            _jwtKey = jwtKey;
            _environment = environment;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environment);
            // UseSetting injeta a chave no host configuration (visível ao Program em minimal hosting);
            // pisa qualquer Jwt:Key vinda de appsettings/env e fixa o valor do caso.
            builder.UseSetting("Jwt:Key", _jwtKey);
        }
    }
}
