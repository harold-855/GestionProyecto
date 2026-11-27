using GestionProyecto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionProyecto.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// Crea instancias aisladas de GestionProyectoDbContext sobre EF Core InMemory para cada prueba.
/// </summary>
internal static class DbContextTestFactory
{
    public static GestionProyectoDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<GestionProyectoDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"InfraTest_{Guid.NewGuid()}")
            .Options;

        return new GestionProyectoDbContext(options);
    }
}
