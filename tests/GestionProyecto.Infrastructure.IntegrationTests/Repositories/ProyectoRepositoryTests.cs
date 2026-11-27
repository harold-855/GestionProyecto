using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using GestionProyecto.Infrastructure.IntegrationTests.Fixtures;
using GestionProyecto.Infrastructure.Repositories;

namespace GestionProyecto.Infrastructure.IntegrationTests.Repositories;

public class ProyectoRepositoryTests
{
    [Fact]
    public async Task AgregarYConsultas_RetornanProyectoConYTareasIncluidas()
    {
        var dbName = $"ProyectoRepo_{Guid.NewGuid()}";
        var proyecto = new Proyecto("Portal Web", "Migración", DateTime.UtcNow, DateTime.UtcNow.AddDays(15));
        var tarea = new Tarea("Maquetar UI", "Diseño responsive", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(5), proyecto.Id);

        await using (var writeContext = DbContextTestFactory.Create(dbName))
        {
            var repo = new ProyectoRepository(writeContext);
            await repo.AgregarAsync(proyecto);
            await writeContext.Tareas.AddAsync(tarea);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = DbContextTestFactory.Create(dbName);
        var readRepo = new ProyectoRepository(readContext);

        var sinTareas = await readRepo.ObtenerPorIdAsync(proyecto.Id);
        var conTareas = await readRepo.ObtenerPorIdConTareasAsync(proyecto.Id);
        var todos = await readRepo.ObtenerTodosAsync();

        Assert.NotNull(sinTareas);
        Assert.NotNull(conTareas);
        Assert.Single(conTareas.Tareas);
        var unico = Assert.Single(todos);
        Assert.Single(unico.Tareas);
    }

    [Fact]
    public async Task ActualizarYEliminar_PersistenCambiosYRemuevenProyecto()
    {
        var dbName = $"ProyectoRepoUpdate_{Guid.NewGuid()}";
        var proyecto = new Proyecto("Inicial", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));

        await using (var context = DbContextTestFactory.Create(dbName))
        {
            var repo = new ProyectoRepository(context);
            await repo.AgregarAsync(proyecto);
            await context.SaveChangesAsync();

            proyecto.Actualizar("Editado", "Nueva desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(20));
            repo.Actualizar(proyecto);
            await context.SaveChangesAsync();
        }

        await using (var verifyContext = DbContextTestFactory.Create(dbName))
        {
            var repo = new ProyectoRepository(verifyContext);
            var actualizado = await repo.ObtenerPorIdAsync(proyecto.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("Editado", actualizado.Nombre);

            repo.Eliminar(actualizado);
            await verifyContext.SaveChangesAsync();
        }

        await using var finalContext = DbContextTestFactory.Create(dbName);
        var finalRepo = new ProyectoRepository(finalContext);
        Assert.Null(await finalRepo.ObtenerPorIdAsync(proyecto.Id));
    }
}
