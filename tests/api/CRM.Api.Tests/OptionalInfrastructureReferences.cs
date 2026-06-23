using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace CRM.Api.Tests;

// Infrastructure placeholders to keep required toolchain available for future deeper tests.
public static class OptionalInfrastructureReferences
{
    public static PostgreSqlContainer BuildPostgresContainer() =>
        new PostgreSqlBuilder()
            .WithDatabase("crm_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public static Task<Respawner> BuildRespawnerAsync(DbConnection connection) =>
        Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
        });
}
