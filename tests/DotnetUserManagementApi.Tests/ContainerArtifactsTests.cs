using System.Text.RegularExpressions;

namespace DotnetUserManagementApi.Tests;

/// <summary>
/// Invariantes de segurança da containerização da Fase 2:
/// Dockerfile multi-stage sem segredos, compose prod-like (postgres:16 + volume + healthcheck +
/// fail-fast JWT__KEY + db sem porta publicada), .env.example só placeholders, .dockerignore/.gitignore
/// excluindo o .env.
/// </summary>
public sealed class ContainerArtifactsTests
{
    private static readonly string Root = RepoPath.Root;

    private static readonly string DockerfilePath = Path.Combine(Root, "Dockerfile");
    private static readonly string ComposePath = Path.Combine(Root, "docker-compose.yml");
    private static readonly string EnvExamplePath = Path.Combine(Root, ".env.example");
    private static readonly string DockerignorePath = Path.Combine(Root, ".dockerignore");
    private static readonly string GitignorePath = Path.Combine(Root, ".gitignore");

    private static string Read(string path) => File.ReadAllText(path);

    private static string[] ReadLines(string path) => File.ReadAllLines(path);

    [Fact]
    public void Dockerfile_MultiStage_BuildsFromSdk8AndRunsOnAspnet8Alpine()
    {
        var content = Read(DockerfilePath);

        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:8.0", content);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine", content);
        Assert.Contains("EXPOSE 8080", content);
    }

    [Fact]
    public void Dockerfile_HasNoBakedInSecrets()
    {
        var lines = ReadLines(DockerfilePath);

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("ENV ") || trimmed.StartsWith("ARG "))
            {
                Assert.False(
                    trimmed.Contains("Jwt", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Contains("KEY", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase),
                    $"Dockerfile não pode conter segredo em ENV/ARG: '{line}'");
            }
        }

        // Nenhuma referência a JWT__KEY ou a credenciais em nenhum ponto do Dockerfile
        Assert.DoesNotContain("JWT__KEY", Read(DockerfilePath), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compose_DbService_UsesPostgres16()
    {
        Assert.Contains("postgres:16", Read(ComposePath));
    }

    [Fact]
    public void Compose_HasNamedVolumePostgresData()
    {
        var content = Read(ComposePath);

        // volume nomeado (D-06): pelo menos mount no db + declaração no nível raiz
        Assert.True(CountOccurrences(content, "postgres_data") >= 2,
            "postgres_data deve aparecer ao menos 2 vezes (mount + declaração do volume).");
    }

    [Fact]
    public void Compose_DbService_HealthcheckUsesPgIsReady()
    {
        Assert.Contains("pg_isready", Read(ComposePath));
    }

    [Fact]
    public void Compose_ApiDependsOnHealthyDb()
    {
        Assert.Contains("service_healthy", Read(ComposePath));
    }

    [Fact]
    public void Compose_ApiEnvironment_IsProductionNeverDevelopment()
    {
        var content = Read(ComposePath);

        Assert.Contains("ASPNETCORE_ENVIRONMENT: Production", content);
        Assert.DoesNotContain("Development", content);
    }

    [Fact]
    public void Compose_ApiJwtKey_UsesFailFastInterpolation()
    {
        // ${JWT__KEY:?} aborta o compose se a variável não estiver definida (D-08)
        Assert.Contains("Jwt__Key: ${JWT__KEY:?}", Read(ComposePath));
    }

    [Fact]
    public void Compose_DbService_HasNoPublishedPorts()
    {
        // T-02-04: o PostgreSQL fica apenas na rede interna do compose — sem bloco ports: no serviço db
        var lines = ReadLines(ComposePath);

        var dbIndex = Array.FindIndex(lines, line => line.Trim() == "db:");
        var apiIndex = Array.FindIndex(lines, line => line.Trim() == "api:");

        Assert.True(dbIndex >= 0, "Serviço 'db' deve existir no compose.");
        Assert.True(apiIndex > dbIndex, "Serviço 'api' deve vir depois do serviço 'db'.");

        for (var i = dbIndex; i < apiIndex; i++)
        {
            var trimmed = lines[i].Trim();
            Assert.False(trimmed.StartsWith("ports:"),
                $"Serviço 'db' não pode publicar portas no host: '{lines[i]}'");
        }
    }

    [Fact]
    public void EnvExample_ContainsOnlyPlaceholders()
    {
        var content = Read(EnvExamplePath);

        Assert.Contains("JWT__KEY=changeme", content);
        Assert.Contains("POSTGRES_PASSWORD=changeme", content);

        // Nenhum valor real de segredo: sem hex de 64 chars (ex.: openssl rand -hex 32) e sem senha não-placeholder
        Assert.False(Regex.IsMatch(content, @"[0-9a-fA-F]{64}"),
            ".env.example não pode conter valores hex de 64 caracteres (chaves/senhas reais).");
    }

    [Fact]
    public void Dockerignore_ExcludesDotEnvAndGit()
    {
        var lines = ReadLines(DockerignorePath);

        Assert.True(lines.Any(line => Regex.IsMatch(line, @"^\.env$")),
            ".dockerignore deve excluir o .env do build context (T-02-03).");
        Assert.Contains(".git", lines);
    }

    [Fact]
    public void Gitignore_HasSecretsSectionWithDotEnv()
    {
        var lines = ReadLines(GitignorePath);

        var envIndex = Array.FindIndex(lines, line => Regex.IsMatch(line, @"^\.env$"));
        Assert.True(envIndex >= 0, ".gitignore deve conter a linha '.env' (D-09).");

        var secretsSectionIndex = Array.FindIndex(lines, line =>
            line.Contains("secrets", StringComparison.OrdinalIgnoreCase));
        Assert.True(secretsSectionIndex >= 0, ".gitignore deve ter uma seção de segredos.");
        Assert.True(envIndex > secretsSectionIndex,
            "A linha '.env' deve estar dentro da seção de segredos do .gitignore.");
    }

    private static int CountOccurrences(string content, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = content.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
