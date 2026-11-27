using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using GestionProyecto.Infrastructure.IntegrationTests.Fixtures;
using GestionProyecto.Infrastructure.Repositories;

namespace GestionProyecto.Infrastructure.IntegrationTests.Repositories;

public class TareaRepositoryTests
{
    [Fact]
    public async Task ConsultasYFiltros_RetornanSubconjuntosEsperados()
    {
        var dbName = $"TareaRepo_{Guid.NewGuid()}";
        var p1 = new Proyecto("P1", "D1", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(20));
        var p2 = new Proyecto("P2", "D2", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(20));

        var vencidaPendiente = new Tarea("T1", "D1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(-2), p1.Id);
        var enProgresoMedia = new Tarea("T2", "D2", PrioridadTarea.Media, DateTime.UtcNow.AddDays(4), p1.Id);
        enProgresoMedia.CambiarEstado(EstadoTarea.EnProgreso);
        var vencidaCompletada = new Tarea("T3", "D3", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(-3), p2.Id);
        vencidaCompletada.CambiarEstado(EstadoTarea.Completada);

        await using (var seedContext = DbContextTestFactory.Create(dbName))
        {
            await seedContext.Proyectos.AddRangeAsync(p1, p2);
            var repo = new TareaRepository(seedContext);
            await repo.AgregarAsync(vencidaPendiente);
            await repo.AgregarAsync(enProgresoMedia);
            await repo.AgregarAsync(vencidaCompletada);
            await seedContext.SaveChangesAsync();
        }

        await using var readContext = DbContextTestFactory.Create(dbName);
        var readRepo = new TareaRepository(readContext);

        Assert.Equal(3, (await readRepo.ObtenerTodasAsync()).Count);
        Assert.Equal(2, (await readRepo.ObtenerPorProyectoAsync(p1.Id)).Count);
        Assert.Single(await readRepo.ObtenerPorEstadoAsync(EstadoTarea.EnProgreso));
        Assert.Equal(2, (await readRepo.ObtenerPorPrioridadAsync(PrioridadTarea.Alta)).Count);
        Assert.Single(await readRepo.ObtenerPorFiltroAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta));

        var vencidas = await readRepo.ObtenerVencidasAsync();
        var unicaVencida = Assert.Single(vencidas);
        Assert.Equal(vencidaPendiente.Id, unicaVencida.Id);
    }

    [Fact]
    public async Task ActualizarYEliminar_PersistenCambios()
    {
        var dbName = $"TareaRepoCrud_{Guid.NewGuid()}";
        var tarea = new Tarea("Inicial", "Desc", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(5), Guid.NewGuid());

        await using (var context = DbContextTestFactory.Create(dbName))
        {
            var repo = new TareaRepository(context);
            await repo.AgregarAsync(tarea);
            await context.SaveChangesAsync();

            tarea.Actualizar("Editada", "Desc editada", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(8));
            repo.Actualizar(tarea);
            await context.SaveChangesAsync();
        }

        await using (var verifyContext = DbContextTestFactory.Create(dbName))
        {
            var repo = new TareaRepository(verifyContext);
            var guardada = await repo.ObtenerPorIdAsync(tarea.Id);
            Assert.NotNull(guardada);
            Assert.Equal("Editada", guardada.Titulo);

            repo.Eliminar(guardada);
            await verifyContext.SaveChangesAsync();
        }

        await using var finalContext = DbContextTestFactory.Create(dbName);
        var finalRepo = new TareaRepository(finalContext);
        Assert.Null(await finalRepo.ObtenerPorIdAsync(tarea.Id));
    }
}
