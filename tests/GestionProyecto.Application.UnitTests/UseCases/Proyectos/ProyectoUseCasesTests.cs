using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.UseCases.Proyectos;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using Moq;

namespace GestionProyecto.Application.UnitTests.UseCases.Proyectos;

public class ProyectoUseCasesTests
{
    private readonly Mock<IProyectoRepository> _proyectoRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CrearProyecto_Valido_GuardaYRetornaDto()
    {
        var useCase = new CrearProyectoUseCase(_proyectoRepoMock.Object, _unitOfWorkMock.Object);
        var dto = new CrearProyectoDto
        {
            Nombre = "Nuevo Proyecto",
            Descripcion = "Descripción",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(30)
        };

        var resultado = await useCase.EjecutarAsync(dto);

        Assert.Equal("Nuevo Proyecto", resultado.Nombre);
        Assert.Equal(0m, resultado.PorcentajeAvance);
        _proyectoRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Proyecto>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerProyectos_RetornaListaConAvanceCalculado()
    {
        var proyecto = new Proyecto("P1", "D1", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var t1 = new Tarea("T1", "D1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(2), proyecto.Id);
        var t2 = new Tarea("T2", "D2", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(4), proyecto.Id);
        t1.CambiarEstado(EstadoTarea.Completada);
        proyecto.AgregarTarea(t1);
        proyecto.AgregarTarea(t2);

        _proyectoRepoMock.Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(new List<Proyecto> { proyecto });

        var useCase = new ObtenerProyectosUseCase(_proyectoRepoMock.Object);
        var resultado = await useCase.EjecutarAsync();

        var item = Assert.Single(resultado);
        Assert.Equal(50m, item.PorcentajeAvance);
        Assert.Equal(2, item.TotalTareas);
        Assert.Equal(1, item.TareasCompletadas);
    }

    [Fact]
    public async Task ObtenerProyectoPorId_Y_Avance_CuandoNoExiste_RetornanNull()
    {
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdConTareasAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Proyecto?)null);

        var obtenerPorId = new ObtenerProyectoPorIdUseCase(_proyectoRepoMock.Object);
        var obtenerAvance = new ObtenerAvanceProyectoUseCase(_proyectoRepoMock.Object);

        Assert.Null(await obtenerPorId.EjecutarAsync(Guid.NewGuid()));
        Assert.Null(await obtenerAvance.EjecutarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerProyectoPorId_Y_Avance_CuandoExiste_RetornanDtosCalculados()
    {
        var proyecto = new Proyecto("P1", "D1", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var t1 = new Tarea("T1", "D1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(2), proyecto.Id);
        t1.CambiarEstado(EstadoTarea.Completada);
        proyecto.AgregarTarea(t1);

        _proyectoRepoMock.Setup(r => r.ObtenerPorIdConTareasAsync(proyecto.Id)).ReturnsAsync(proyecto);

        var obtenerPorId = new ObtenerProyectoPorIdUseCase(_proyectoRepoMock.Object);
        var obtenerAvance = new ObtenerAvanceProyectoUseCase(_proyectoRepoMock.Object);

        var proyectoDto = await obtenerPorId.EjecutarAsync(proyecto.Id);
        var avanceDto = await obtenerAvance.EjecutarAsync(proyecto.Id);

        Assert.NotNull(proyectoDto);
        Assert.Equal(100m, proyectoDto.PorcentajeAvance);
        Assert.NotNull(avanceDto);
        Assert.Equal(1, avanceDto.TotalTareas);
        Assert.Equal(1, avanceDto.TareasCompletadas);
        Assert.Equal(100m, avanceDto.PorcentajeAvance);
    }

    [Fact]
    public async Task ActualizarProyecto_CuandoExiste_ActualizaYRetornaTrue()
    {
        var proyecto = new Proyecto("Viejo", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(proyecto.Id)).ReturnsAsync(proyecto);

        var useCase = new ActualizarProyectoUseCase(_proyectoRepoMock.Object, _unitOfWorkMock.Object);
        var ok = await useCase.EjecutarAsync(proyecto.Id, new ActualizarProyectoDto
        {
            Nombre = "Actualizado",
            Descripcion = "Nueva",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(15)
        });

        Assert.True(ok);
        Assert.Equal("Actualizado", proyecto.Nombre);
        _proyectoRepoMock.Verify(r => r.Actualizar(proyecto), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarProyecto_CuandoNoExiste_RetornaFalse()
    {
        _proyectoRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Proyecto?)null);

        var useCase = new ActualizarProyectoUseCase(_proyectoRepoMock.Object, _unitOfWorkMock.Object);
        var ok = await useCase.EjecutarAsync(Guid.NewGuid(), new ActualizarProyectoDto
        {
            Nombre = "X",
            Descripcion = "Y",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(2)
        });

        Assert.False(ok);
    }

    [Fact]
    public async Task EliminarProyecto_ConTareasPendientes_LanzaBusinessRuleException()
    {
        var proyecto = new Proyecto("P1", "D1", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        proyecto.AgregarTarea(new Tarea("T1", "D1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(2), proyecto.Id));

        _proyectoRepoMock.Setup(r => r.ObtenerPorIdConTareasAsync(proyecto.Id)).ReturnsAsync(proyecto);

        var useCase = new EliminarProyectoUseCase(_proyectoRepoMock.Object, _unitOfWorkMock.Object);

        await Assert.ThrowsAsync<BusinessRuleException>(() => useCase.EjecutarAsync(proyecto.Id));
        _proyectoRepoMock.Verify(r => r.Eliminar(It.IsAny<Proyecto>()), Times.Never);
    }

    [Fact]
    public async Task EliminarProyecto_SoloTareasCompletadas_EliminaYRetornaTrue()
    {
        var proyecto = new Proyecto("P1", "D1", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var tarea = new Tarea("T1", "D1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(2), proyecto.Id);
        tarea.CambiarEstado(EstadoTarea.Completada);
        proyecto.AgregarTarea(tarea);

        _proyectoRepoMock.Setup(r => r.ObtenerPorIdConTareasAsync(proyecto.Id)).ReturnsAsync(proyecto);

        var useCase = new EliminarProyectoUseCase(_proyectoRepoMock.Object, _unitOfWorkMock.Object);
        var resultado = await useCase.EjecutarAsync(proyecto.Id);

        Assert.True(resultado);
        _proyectoRepoMock.Verify(r => r.Eliminar(proyecto), Times.Once);
        _unitOfWorkMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
