using GestionProyecto.Domain.Entities;
using GestionProyecto.Infrastructure.IntegrationTests.Fixtures;
using GestionProyecto.Infrastructure.Repositories;

namespace GestionProyecto.Infrastructure.IntegrationTests.Repositories;

public class UnitOfWorkTests
{
    [Fact]
    public async Task GuardarCambiosAsync_PersisteEntidadesYRetornaConteoAfectado()
    {
        var dbName = $"UnitOfWork_{Guid.NewGuid()}";
        var proyecto = new Proyecto("UoW Test", "Persistencia", DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        await using (var context = DbContextTestFactory.Create(dbName))
        {
            var repo = new ProyectoRepository(context);
            var unitOfWork = new UnitOfWork(context);

            await repo.AgregarAsync(proyecto);
            var cambios = await unitOfWork.GuardarCambiosAsync();

            Assert.Equal(1, cambios);
        }

        await using var readContext = DbContextTestFactory.Create(dbName);
        var guardado = await new ProyectoRepository(readContext).ObtenerPorIdAsync(proyecto.Id);
        Assert.NotNull(guardado);
        Assert.Equal("UoW Test", guardado.Nombre);
    }
}
