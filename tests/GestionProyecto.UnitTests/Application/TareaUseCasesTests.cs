using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.UseCases.Tareas;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using Moq;
using Xunit;

namespace GestionProyecto.UnitTests.Application;

public class TareaUseCasesTests
{
    private readonly Mock<ITareaRepository> _tareaRepoMock = new();
    private readonly Mock<IProyectoRepository> _proyectoRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CrearTarea_CuandoProyectoNoExiste_LanzaNotFoundException()
    {
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Proyecto?)null);

        var useCase = new CrearTareaUseCase(
            _tareaRepoMock.Object,
            _proyectoRepoMock.Object,
            _unitOfWorkMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.EjecutarAsync(new CrearTareaDto
        {
            Titulo = "Tarea",
            Descripcion = "Desc",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = Guid.NewGuid()
        }));
    }

    [Fact]
    public async Task CrearTarea_CuandoProyectoExiste_CreaYRetornaDto()
    {
        var proyecto = new Proyecto("P", "D", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(proyecto.Id)).ReturnsAsync(proyecto);

        var useCase = new CrearTareaUseCase(
            _tareaRepoMock.Object,
            _proyectoRepoMock.Object,
            _unitOfWorkMock.Object);

        var resultado = await useCase.EjecutarAsync(new CrearTareaDto
        {
            Titulo = "Implementar tests",
            Descripcion = "Unitarios e integrales",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto.Id
        });

        Assert.Equal("Implementar tests", resultado.Titulo);
        Assert.Equal(EstadoTarea.Pendiente, resultado.Estado);
        Assert.Equal(proyecto.Id, resultado.ProyectoId);
        _tareaRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Tarea>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoTarea_CuandoExiste_CambiaYGuarda()
    {
        var tarea = new Tarea("T", "D", PrioridadTarea.Media, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        _tareaRepoMock.Setup(r => r.ObtenerPorIdAsync(tarea.Id)).ReturnsAsync(tarea);

        var useCase = new CambiarEstadoTareaUseCase(_tareaRepoMock.Object, _unitOfWorkMock.Object);
        var ok = await useCase.EjecutarAsync(tarea.Id, EstadoTarea.Completada);

        Assert.True(ok);
        Assert.Equal(EstadoTarea.Completada, tarea.Estado);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
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

    [Fact]
    public async Task ActualizarYEliminarTarea_CuandoNoExiste_RetornanFalse()
    {
        _tareaRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Tarea?)null);

        var actualizar = new ActualizarTareaUseCase(_tareaRepoMock.Object, _unitOfWorkMock.Object);
        var eliminar = new EliminarTareaUseCase(_tareaRepoMock.Object, _unitOfWorkMock.Object);

        Assert.False(await actualizar.EjecutarAsync(Guid.NewGuid(), new ActualizarTareaDto
        {
            Titulo = "T",
            Descripcion = "D",
            Prioridad = PrioridadTarea.Baja,
            FechaLimite = DateTime.UtcNow.AddDays(1)
        }));
        Assert.False(await eliminar.EjecutarAsync(Guid.NewGuid()));
    }
}
