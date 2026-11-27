using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Domain.UnitTests.Entities;

public class ProyectoTests
{
    private static readonly DateTime InicioValido = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FinValida = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CrearProyecto_ConDatosValidos_NormalizaTextoYFechasUtc()
    {
        var proyecto = new Proyecto(
            "  Proyecto Alpha  ",
            "  Descripción general  ",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 2, 1));

        Assert.NotEqual(Guid.Empty, proyecto.Id);
        Assert.Equal("Proyecto Alpha", proyecto.Nombre);
        Assert.Equal("Descripción general", proyecto.Descripcion);
        Assert.Equal(DateTimeKind.Utc, proyecto.FechaInicio.Kind);
        Assert.Equal(DateTimeKind.Utc, proyecto.FechaFinalizacion.Kind);
        Assert.Empty(proyecto.Tareas);
    }

    [Theory]
    [InlineData("", "Descripción")]
    [InlineData("   ", "Descripción")]
    [InlineData("Nombre", "")]
    [InlineData("Nombre", "   ")]
    public void CrearProyecto_ConNombreODescripcionInvalidos_LanzaArgumentException(
        string nombre,
        string descripcion)
    {
        Assert.Throws<ArgumentException>(() =>
            new Proyecto(nombre, descripcion, InicioValido, FinValida));
    }

    [Fact]
    public void CrearProyecto_ConFechaFinalizacionAnteriorOIgualAInicio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Proyecto("Proyecto", "Desc", InicioValido, InicioValido));

        Assert.Throws<ArgumentException>(() =>
            new Proyecto("Proyecto", "Desc", FinValida, InicioValido));
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaPropiedades()
    {
        var proyecto = new Proyecto("Original", "Desc", InicioValido, FinValida);
        var nuevoInicio = InicioValido.AddDays(5);
        var nuevoFin = FinValida.AddDays(10);

        proyecto.Actualizar("  Nuevo Nombre ", " Nueva Desc ", nuevoInicio, nuevoFin);

        Assert.Equal("Nuevo Nombre", proyecto.Nombre);
        Assert.Equal("Nueva Desc", proyecto.Descripcion);
        Assert.Equal(nuevoInicio, proyecto.FechaInicio);
        Assert.Equal(nuevoFin, proyecto.FechaFinalizacion);
    }

    [Fact]
    public void CrearProyecto_ConFechaLocal_NormalizaAUtc()
    {
        var inicioLocal = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Local);
        var finLocal = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Local);

        var proyecto = new Proyecto("Local", "Desc", inicioLocal, finLocal);

        Assert.Equal(DateTimeKind.Utc, proyecto.FechaInicio.Kind);
        Assert.Equal(DateTimeKind.Utc, proyecto.FechaFinalizacion.Kind);
        Assert.Equal(inicioLocal.ToUniversalTime(), proyecto.FechaInicio);
        Assert.Equal(finLocal.ToUniversalTime(), proyecto.FechaFinalizacion);
    }

    [Fact]
    public void Actualizar_ConDatosInvalidos_LanzaArgumentException()
    {
        var proyecto = new Proyecto("Original", "Desc", InicioValido, FinValida);

        Assert.Throws<ArgumentException>(() => proyecto.Actualizar("", "Desc", InicioValido, FinValida));
        Assert.Throws<ArgumentException>(() => proyecto.Actualizar("Nombre", " ", InicioValido, FinValida));
        Assert.Throws<ArgumentException>(() => proyecto.Actualizar("Nombre", "Desc", FinValida, InicioValido));
    }

    [Fact]
    public void AgregarTarea_CuandoEsNull_LanzaArgumentNullException()
    {
        var proyecto = new Proyecto("Proyecto", "Desc", InicioValido, FinValida);

        Assert.Throws<ArgumentNullException>(() => proyecto.AgregarTarea(null!));
    }

    [Fact]
    public void CalcularPorcentajeAvance_SinTareas_RetornaCero()
    {
        var proyecto = new Proyecto("Proyecto", "Desc", InicioValido, FinValida);

        Assert.Equal(0m, proyecto.CalcularPorcentajeAvance());
    }

    [Fact]
    public void CalcularPorcentajeAvance_ConTareasMixtas_RetornaPorcentajeRedondeado()
    {
        var proyecto = new Proyecto("Proyecto", "Desc", InicioValido, FinValida);
        var t1 = new Tarea("T1", "D1", PrioridadTarea.Alta, FinValida, proyecto.Id);
        var t2 = new Tarea("T2", "D2", PrioridadTarea.Media, FinValida, proyecto.Id);
        var t3 = new Tarea("T3", "D3", PrioridadTarea.Baja, FinValida, proyecto.Id);
        t1.CambiarEstado(EstadoTarea.Completada);
        t2.CambiarEstado(EstadoTarea.EnProgreso);

        proyecto.AgregarTarea(t1);
        proyecto.AgregarTarea(t2);
        proyecto.AgregarTarea(t3);

        Assert.Equal(33.33m, proyecto.CalcularPorcentajeAvance());
    }

    [Theory]
    [InlineData(EstadoTarea.Pendiente, true)]
    [InlineData(EstadoTarea.EnProgreso, true)]
    [InlineData(EstadoTarea.Completada, false)]
    public void TieneTareasPendientes_SegunEstadoDeTareas_RetornaResultadoEsperado(
        EstadoTarea estado,
        bool esperado)
    {
        var proyecto = new Proyecto("Proyecto", "Desc", InicioValido, FinValida);
        var tarea = new Tarea("T1", "D1", PrioridadTarea.Media, FinValida, proyecto.Id);
        tarea.CambiarEstado(estado);
        proyecto.AgregarTarea(tarea);

        Assert.Equal(esperado, proyecto.TieneTareasPendientes());
    }

    [Fact]
    public void AgregarYRetirarTarea_ValidacionesDePertenenciaYDuplicados()
    {
        var proyecto = new Proyecto("Proyecto", "Desc", InicioValido, FinValida);
        var tareaAjena = new Tarea("Ajena", "Desc", PrioridadTarea.Baja, FinValida, Guid.NewGuid());
        var tareaPropia = new Tarea("Propia", "Desc", PrioridadTarea.Alta, FinValida, proyecto.Id);

        Assert.Throws<InvalidOperationException>(() => proyecto.AgregarTarea(tareaAjena));

        proyecto.AgregarTarea(tareaPropia);
        Assert.Throws<InvalidOperationException>(() => proyecto.AgregarTarea(tareaPropia));

        proyecto.RetirarTarea(tareaPropia.Id);
        Assert.Empty(proyecto.Tareas);

        Assert.Throws<InvalidOperationException>(() => proyecto.RetirarTarea(tareaPropia.Id));
    }
}
