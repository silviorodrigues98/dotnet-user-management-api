using System.Text.RegularExpressions;

namespace DotnetUserManagementApi.Tests;

/// <summary>
/// Critérios de aceite de docs/CI da Fase 2 (D-10/D-11/D-12/D-13/D-14):
/// ci-cd.yml (build+test, sem SonarQube, sem secrets, push main + PRs), ARCHITECTURE.md
/// (entregável com diagramas Mermaid, ≥120 linhas, sem segredos) e README.md (seção Docker,
/// tabela dual-provider, sem SonarQube).
/// </summary>
public sealed class DocumentationArtifactsTests
{
    private static readonly string Root = RepoPath.Root;

    private static readonly string WorkflowPath = Path.Combine(Root, ".github", "workflows", "ci-cd.yml");
    private static readonly string ArchitecturePath = Path.Combine(Root, "ARCHITECTURE.md");
    private static readonly string ReadmePath = Path.Combine(Root, "README.md");

    private static string Read(string path) => File.ReadAllText(path);

    [Fact]
    public void Workflow_TriggersOnPushMainAndPullRequest()
    {
        var content = Read(WorkflowPath);

        Assert.Contains("pull_request", content);
        Assert.Contains("main", content);
        Assert.Contains("branches: [main]", content);
    }

    [Fact]
    public void Workflow_UsesSetupDotnetV4_WithSdk80x()
    {
        var content = Read(WorkflowPath);

        Assert.Contains("actions/setup-dotnet@v4", content);
        Assert.Contains("dotnet-version: '8.0.x'", content);
    }

    [Fact]
    public void Workflow_TestStep_RunsDotnetTestWithNoBuild()
    {
        var content = Read(WorkflowPath);

        Assert.Contains("dotnet test", content);
        Assert.Contains("--no-build", content);
    }

    [Fact]
    public void Workflow_HasNoSonarQube_AndNoSecrets()
    {
        var content = Read(WorkflowPath);

        // D-10: sem análise estática SonarQube; T-02-07: sem material secreto no job
        Assert.DoesNotContain("SonarQube", content, StringComparison.Ordinal);
        Assert.DoesNotContain("${{ secrets.", content, StringComparison.Ordinal);
    }

    [Fact]
    public void ArchitectureDoc_HasAtLeastThreeMermaidDiagrams_WithSequenceDiagram()
    {
        var content = Read(ArchitecturePath);

        // D-13: arquitetura + fluxo de autenticação + deploy compose
        Assert.True(CountOccurrences(content, "```mermaid") >= 3,
            "ARCHITECTURE.md deve conter ao menos 3 diagramas Mermaid.");
        Assert.Contains("sequenceDiagram", content);
    }

    [Fact]
    public void ArchitectureDoc_DescribesRealDeployTopology()
    {
        var content = Read(ArchitecturePath);

        Assert.Contains("postgres_data", content);
        Assert.Contains("JWT__KEY", content);
        Assert.Contains("ConnectionStrings__Database", content);
    }

    [Fact]
    public void ArchitectureDoc_IsSubstantial_AndLeaksNoSecrets()
    {
        var content = Read(ArchitecturePath);
        var lineCount = File.ReadLines(ArchitecturePath).Count();

        Assert.True(lineCount >= 120, $"ARCHITECTURE.md deve ter >= 120 linhas (tem {lineCount}).");

        // D-10: nenhuma menção ao SonarQube (nem como recurso planejado)
        Assert.DoesNotContain("SonarQube", content, StringComparison.Ordinal);

        // T-02-08: nenhum valor real de segredo — sem hex de 64 chars nem "Password=" com valor
        Assert.False(Regex.IsMatch(content, @"[0-9a-fA-F]{64}"),
            "ARCHITECTURE.md não pode conter valores hex de 64 caracteres.");
        Assert.False(Regex.IsMatch(content, @"Password\s*=\s*\S"),
            "ARCHITECTURE.md não pode conter valores de senha em connection strings.");
    }

    [Fact]
    public void Readme_HasDockerRunSection()
    {
        var content = Read(ReadmePath);

        Assert.Contains("Rodar com Docker (PostgreSQL)", content);
        Assert.Contains("docker compose up --build", content);
        Assert.Contains("postgres_data", content);
        Assert.Contains("http://localhost:5290", content);
    }

    [Fact]
    public void Readme_HasNoSonarQube_AndDocumentsDualProviderDecision()
    {
        var content = Read(ReadmePath);

        // D-10: README sem SonarQube
        Assert.DoesNotContain("SonarQube", content, StringComparison.Ordinal);

        // D-01: tabela de decisões com a linha dual-provider (SQLite local / PostgreSQL Docker)
        Assert.Contains("EF Core dual-provider", content);
        Assert.Contains("SQLite local / PostgreSQL Docker", content);
        Assert.Contains("ConnectionStrings:Database", content);
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
