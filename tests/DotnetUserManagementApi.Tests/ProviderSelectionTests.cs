using DotnetUserManagementApi.Infrastructure;
using DotnetUserManagementApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace DotnetUserManagementApi.Tests;

/// <summary>
/// D-01/D-03/D-04: AddInfrastructure seleciona o provider pelo valor de ConnectionStrings:Database
/// ("Postgres" OrdinalIgnoreCase → Npgsql; ausente/"Sqlite"/qualquer outro → SQLite).
/// </summary>
public sealed class ProviderSelectionTests
{
    [Fact]
    public void AddInfrastructure_DatabaseIsPostgres_SelectsNpgsqlProvider()
    {
        var options = BuildOptions("Postgres");

        Assert.True(options.Extensions.OfType<NpgsqlOptionsExtension>().Any(),
            "Esperado provider Npgsql (NpgsqlOptionsExtension) para ConnectionStrings:Database=Postgres.");
        Assert.False(options.Extensions.OfType<SqliteOptionsExtension>().Any(),
            "Npgsql não pode coexistir com SqliteOptionsExtension.");
    }

    [Fact]
    public void AddInfrastructure_DatabaseIsPostgresDifferentCase_SelectsNpgsqlProvider()
    {
        // D-01: comparação OrdinalIgnoreCase — "postgres", "POSTGRES", "PoStGrEs" também selecionam Npgsql
        foreach (var value in new[] { "postgres", "POSTGRES", "PoStGrEs" })
        {
            var options = BuildOptions(value);

            Assert.True(options.Extensions.OfType<NpgsqlOptionsExtension>().Any(),
                $"Esperado provider Npgsql para ConnectionStrings:Database={value} (OrdinalIgnoreCase).");
        }
    }

    [Fact]
    public void AddInfrastructure_DatabaseMissing_SelectsSqliteProvider()
    {
        // D-04: default local preservado quando a chave está ausente
        var options = BuildOptions(database: null);

        Assert.True(options.Extensions.OfType<SqliteOptionsExtension>().Any(),
            "Esperado provider SQLite (SqliteOptionsExtension) quando ConnectionStrings:Database está ausente.");
        Assert.False(options.Extensions.OfType<NpgsqlOptionsExtension>().Any());
    }

    [Fact]
    public void AddInfrastructure_DatabaseIsSqlite_SelectsSqliteProvider()
    {
        var options = BuildOptions("Sqlite");

        Assert.True(options.Extensions.OfType<SqliteOptionsExtension>().Any(),
            "Esperado provider SQLite para ConnectionStrings:Database=Sqlite.");
        Assert.False(options.Extensions.OfType<NpgsqlOptionsExtension>().Any());
    }

    [Fact]
    public void AddInfrastructure_DatabaseIsAnyOtherValue_SelectsSqliteProvider()
    {
        // D-04: qualquer valor fora de "Postgres" cai no default SQLite
        var options = BuildOptions("MySQL");

        Assert.True(options.Extensions.OfType<SqliteOptionsExtension>().Any(),
            "Esperado provider SQLite (default) para valor desconhecido de ConnectionStrings:Database.");
        Assert.False(options.Extensions.OfType<NpgsqlOptionsExtension>().Any());
    }

    private static DbContextOptions<AppDbContext> BuildOptions(string? database)
    {
        var configValues = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
        };

        if (database is not null)
        {
            configValues["ConnectionStrings:Database"] = database;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<DbContextOptions<AppDbContext>>();
    }
}
