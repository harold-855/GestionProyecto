using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.UseCases.Tareas;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using Moq;

namespace GestionProyecto.Application.UnitTests.UseCases.Tareas;

public class TareaConsultasUseCasesTests
{
    private readonly Mock<ITareaRepository> _tareaRepoMock = new();
    private readonly Mock<IProyectoRepository> _proyectoRepoMock = new();

    [Fact]
    public async Task ObtenerTareas_Y_PorId_RetornanResultadosMapeados()
    {
        var tarea = new Tarea("T1", "D1", PrioridadTarea.Media, DateTime.UtcNow.AddDays(2), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(new List<Tarea> { tarea });
        _tareaRepoMock.Setup(r => r.ObtenerPorIdAsync(tarea.Id)).ReturnsAsync(tarea);

        var obtenerTodas = new ObtenerTareasUseCase(_tareaRepoMock.Object);
        var obtenerPorId = new ObtenerTareaPorIdUseCase(_tareaRepoMock.Object);

        var lista = await obtenerTodas.EjecutarAsync();
        var item = await obtenerPorId.EjecutarAsync(tarea.Id);

        Assert.Single(lista);
        Assert.NotNull(item);
        Assert.Equal(tarea.Id, item.Id);
    }

    [Fact]
    public async Task ObtenerTareaPorId_CuandoNoExiste_RetornaNull()
    {
        _tareaRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Tarea?)null);

        var obtenerPorId = new ObtenerTareaPorIdUseCase(_tareaRepoMock.Object);

        Assert.Null(await obtenerPorId.EjecutarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerTareasPorProyecto_CuandoProyectoExiste_RetornaTareas()
    {
        var proyecto = new Proyecto("P", "D", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
        var tarea = new Tarea("T", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(2), proyecto.Id);

        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(proyecto.Id)).ReturnsAsync(proyecto);
        _tareaRepoMock.Setup(r => r.ObtenerPorProyectoAsync(proyecto.Id))
            .ReturnsAsync(new List<Tarea> { tarea });

        var useCase = new ObtenerTareasPorProyectoUseCase(_tareaRepoMock.Object, _proyectoRepoMock.Object);
        var resultado = await useCase.EjecutarAsync(proyecto.Id);

        var dto = Assert.Single(resultado);
        Assert.Equal(tarea.Id, dto.Id);
    }

    [Fact]
    public async Task ObtenerTareasPorProyecto_CuandoProyectoNoExiste_LanzaNotFoundException()
    {
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Proyecto?)null);

        var useCase = new ObtenerTareasPorProyectoUseCase(_tareaRepoMock.Object, _proyectoRepoMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.EjecutarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FiltrarTareas_Combinado_ValidaEnumsYConsultaRepositorio()
    {
        var tarea = new Tarea("T", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(3), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerPorFiltroAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta))
            .ReturnsAsync(new List<Tarea> { tarea });

        var useCase = new FiltrarTareasUseCase(_tareaRepoMock.Object);
        var resultado = await useCase.EjecutarAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta);

        Assert.Single(resultado);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.EjecutarAsync((EstadoTarea)99, null));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.EjecutarAsync(null, (PrioridadTarea)99));
    }

    [Theory]
    [InlineData(EstadoTarea.Pendiente, null)]
    [InlineData(null, PrioridadTarea.Alta)]
    [InlineData(null, null)]
    public async Task FiltrarTareas_ConFiltrosParcialesONulos_ConsultaRepositorio(
        EstadoTarea? estado,
        PrioridadTarea? prioridad)
    {
        var tarea = new Tarea("T", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(3), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerPorFiltroAsync(estado, prioridad))
            .ReturnsAsync(new List<Tarea> { tarea });

        var useCase = new FiltrarTareasUseCase(_tareaRepoMock.Object);
        var resultado = await useCase.EjecutarAsync(estado, prioridad);

        Assert.Single(resultado);
        _tareaRepoMock.Verify(r => r.ObtenerPorFiltroAsync(estado, prioridad), Times.Once);
    }

    [Fact]
    public async Task FiltrarPorEstadoYPrioridad_ValidanEnumsYConsultanRepositorio()
    {
        var tarea = new Tarea("T", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(3), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerPorEstadoAsync(EstadoTarea.EnProgreso))
            .ReturnsAsync(new List<Tarea> { tarea });
        _tareaRepoMock.Setup(r => r.ObtenerPorPrioridadAsync(PrioridadTarea.Alta))
            .ReturnsAsync(new List<Tarea> { tarea });

        var porEstado = new FiltrarTareasPorEstadoUseCase(_tareaRepoMock.Object);
        var porPrioridad = new FiltrarTareasPorPrioridadUseCase(_tareaRepoMock.Object);

        Assert.Single(await porEstado.EjecutarAsync(EstadoTarea.EnProgreso));
        Assert.Single(await porPrioridad.EjecutarAsync(PrioridadTarea.Alta));

        await Assert.ThrowsAsync<ArgumentException>(() => porEstado.EjecutarAsync((EstadoTarea)99));
        await Assert.ThrowsAsync<ArgumentException>(() => porPrioridad.EjecutarAsync((PrioridadTarea)99));
    }

    [Fact]
    public async Task ObtenerTareasVencidas_RetornaMapeadasConEstaVencidaTrue()
    {
        var tareaVencida = new Tarea("Vencida", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(-1), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerVencidasAsync()).ReturnsAsync(new List<Tarea> { tareaVencida });

        var useCase = new ObtenerTareasVencidasUseCase(_tareaRepoMock.Object);
        var resultado = await useCase.EjecutarAsync();

        var item = Assert.Single(resultado);
        Assert.True(item.EstaVencida);
    }
}
