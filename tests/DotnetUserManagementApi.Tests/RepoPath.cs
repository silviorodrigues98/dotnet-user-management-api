namespace DotnetUserManagementApi.Tests;

/// <summary>
/// Localiza a raiz do repositório a partir do diretório de execução dos testes,
/// subindo na árvore até encontrar os artefatos de containerização (Dockerfile + docker-compose.yml).
/// </summary>
internal static class RepoPath
{
    public static string Root { get; } = FindRepoRoot();

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Dockerfile"))
                && File.Exists(Path.Combine(current.FullName, "docker-compose.yml")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositório não encontrada a partir de " + AppContext.BaseDirectory);
    }
}
