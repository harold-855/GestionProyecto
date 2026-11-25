using GestionProyecto.Domain.Shared;

namespace GestionProyecto.Domain.Entities;

public class Proyecto : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFinalizacion { get; private set; }

    // Relación: un proyecto puede tener muchas tareas.
    private readonly List<Tarea> _tareas = new();

    public IReadOnlyCollection<Tarea> Tareas => _tareas.AsReadOnly();

    // Constructor privado para Entity Framework Core.
    private Proyecto()
    {
    }

    // Constructor principal.
    public Proyecto(
        string nombre,
        string descripcion,
        DateTime fechaInicio,
        DateTime fechaFinalizacion)
    {
        ValidarDatos(nombre, descripcion, fechaInicio, fechaFinalizacion);

        Nombre = nombre.Trim();
        Descripcion = descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFinalizacion = fechaFinalizacion;
    }

    // Actualizar información del proyecto.
    public void Actualizar(
        string nombre,
        string descripcion,
        DateTime fechaInicio,
        DateTime fechaFinalizacion)
    {
        ValidarDatos(nombre, descripcion, fechaInicio, fechaFinalizacion);

        Nombre = nombre.Trim();
        Descripcion = descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFinalizacion = fechaFinalizacion;
    }

    // Agregar una tarea al proyecto.
    public void AgregarTarea(Tarea tarea)
    {
        ArgumentNullException.ThrowIfNull(tarea);

        if (tarea.ProyectoId != Id)
            throw new InvalidOperationException(
                "La tarea no pertenece a este proyecto.");

        if (_tareas.Any(t => t.Id == tarea.Id))
            throw new InvalidOperationException(
                "La tarea ya está registrada en este proyecto.");

        _tareas.Add(tarea);
    }

    // Retirar una tarea de la colección.
    public void RetirarTarea(Guid tareaId)
    {
        var tarea = _tareas.FirstOrDefault(t => t.Id == tareaId);

        if (tarea is null)
            throw new InvalidOperationException(
                "La tarea no existe en este proyecto.");

        _tareas.Remove(tarea);
    }

    // Calcular porcentaje de avance.
    public decimal CalcularPorcentajeAvance()
    {
        if (_tareas.Count == 0)
            return 0;

        int tareasCompletadas = _tareas.Count(
            t => t.Estado == Enums.EstadoTarea.Completada);

        return Math.Round(
            (decimal)tareasCompletadas / _tareas.Count * 100,
            2);
    }

    // Comprobar si existen tareas pendientes.
    public bool TieneTareasPendientes()
    {
        return _tareas.Any(
            t => t.Estado == Enums.EstadoTarea.Pendiente);
    }

    // Validaciones del proyecto.
    private static void ValidarDatos(
        string nombre,
        string descripcion,
        DateTime fechaInicio,
        DateTime fechaFinalizacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripción del proyecto es obligatoria.");

        if (fechaFinalizacion <= fechaInicio)
            throw new ArgumentException(
                "La fecha de finalización debe ser posterior a la fecha de inicio.");
    }
}