using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using Xunit;

namespace GestionProyecto.UnitTests.Domain;

public class TareaTests
{
    [Fact]
    public void CrearTarea_ConDatosValidos_IniciaPendienteYNormalizaUtc()
    {
        var proyectoId = Guid.NewGuid();
        var fecha = new DateTime(2026, 12, 15, 10, 0, 0);

        var tarea = new Tarea("  Diseñar API ", "  Endpoints REST ", PrioridadTarea.Alta, fecha, proyectoId);

        Assert.NotEqual(Guid.Empty, tarea.Id);
        Assert.Equal("Diseñar API", tarea.Titulo);
        Assert.Equal("Endpoints REST", tarea.Descripcion);
        Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
        Assert.Equal(PrioridadTarea.Alta, tarea.Prioridad);
        Assert.Equal(DateTimeKind.Utc, tarea.FechaLimite.Kind);
        Assert.Equal(proyectoId, tarea.ProyectoId);
    }

    [Fact]
    public void CrearTarea_ConDatosInvalidos_LanzaArgumentException()
    {
        var proyectoId = Guid.NewGuid();
        var fecha = DateTime.UtcNow.AddDays(5);

        Assert.Throws<ArgumentException>(() => new Tarea("", "Desc", PrioridadTarea.Alta, fecha, proyectoId));
        Assert.Throws<ArgumentException>(() => new Tarea("Título", " ", PrioridadTarea.Alta, fecha, proyectoId));
        Assert.Throws<ArgumentException>(() => new Tarea("Título", "Desc", (PrioridadTarea)99, fecha, proyectoId));
        Assert.Throws<ArgumentException>(() => new Tarea("Título", "Desc", PrioridadTarea.Media, default, proyectoId));
        Assert.Throws<ArgumentException>(() => new Tarea("Título", "Desc", PrioridadTarea.Media, fecha, Guid.Empty));
    }

    [Fact]
    public void CambiarEstado_EstadoValidoEInvalido()
    {
        var tarea = new Tarea("T", "D", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        tarea.CambiarEstado(EstadoTarea.EnProgreso);
        Assert.Equal(EstadoTarea.EnProgreso, tarea.Estado);

        tarea.CambiarEstado(EstadoTarea.Completada);
        Assert.Equal(EstadoTarea.Completada, tarea.Estado);

        Assert.Throws<ArgumentException>(() => tarea.CambiarEstado((EstadoTarea)42));
    }

    [Fact]
    public void EstaVencida_EvaluaFechaYEstadoCompletada()
    {
        var tareaPasadaPendiente = new Tarea("T1", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(-2), Guid.NewGuid());
        Assert.True(tareaPasadaPendiente.EstaVencida());

        tareaPasadaPendiente.CambiarEstado(EstadoTarea.Completada);
        Assert.False(tareaPasadaPendiente.EstaVencida());

        var tareaFutura = new Tarea("T2", "D", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(3), Guid.NewGuid());
        Assert.False(tareaFutura.EstaVencida());
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaCampos()
    {
        var tarea = new Tarea("Inicial", "Desc", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var nuevaFecha = DateTime.UtcNow.AddDays(10);

        tarea.Actualizar("  Actualizada ", " Nueva desc ", PrioridadTarea.Alta, nuevaFecha);

        Assert.Equal("Actualizada", tarea.Titulo);
        Assert.Equal("Nueva desc", tarea.Descripcion);
        Assert.Equal(PrioridadTarea.Alta, tarea.Prioridad);
        Assert.Equal(nuevaFecha, tarea.FechaLimite);
    }
}
