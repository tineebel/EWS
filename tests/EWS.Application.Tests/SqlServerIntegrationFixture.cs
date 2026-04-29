using System.Text.Json;
using EWS.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Tests;

[CollectionDefinition(Name)]
public sealed class SqlServerIntegrationCollection : ICollectionFixture<SqlServerIntegrationFixture>
{
    public const string Name = "SqlServerIntegration";
}

public sealed class SqlServerIntegrationFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"EWS_IntegrationTests_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;

    public string ConnectionString => _connectionString;

    public async Task InitializeAsync()
    {
        var baseConnectionString = ReadBaseConnectionString();
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = _databaseName,
            Encrypt = false,
            TrustServerCertificate = true
        };
        _connectionString = builder.ConnectionString;

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString)) return;

        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        builder.Encrypt = false;
        builder.TrustServerCertificate = true;

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END
            """;
        await command.ExecuteNonQueryAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string ReadBaseConnectionString()
    {
        var envConnection = Environment.GetEnvironmentVariable("EWS_TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConnection))
            return envConnection;

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var appsettingsPath = Path.Combine(current.FullName, "src", "EWS.API", "appsettings.json");
            if (File.Exists(appsettingsPath))
            {
                using var stream = File.OpenRead(appsettingsPath);
                using var doc = JsonDocument.Parse(stream);
                return doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("DefaultConnection")
                    .GetString()!;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "SQL Server integration tests require EWS_TEST_CONNECTION_STRING or src/EWS.API/appsettings.json.");
    }
}
