using GestionProyecto.Domain.Enums;
using GestionProyecto.Domain.Shared;

namespace GestionProyecto.Domain.Entities;

public class Tarea : BaseEntity
{
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;

    public EstadoTarea Estado { get; private set; }

    public PrioridadTarea Prioridad { get; private set; }

    public DateTime FechaLimite { get; private set; }

    // Clave foránea del proyecto.
    public Guid ProyectoId { get; private set; }

    // Navegación hacia Proyecto.
    public Proyecto? Proyecto { get; private set; }

    // Constructor privado para Entity Framework Core.
    private Tarea()
    {
    }

    // Constructor principal.
    public Tarea(
        string titulo,
        string descripcion,
        PrioridadTarea prioridad,
        DateTime fechaLimite,
        Guid proyectoId)
    {
        ValidarDatos(titulo, descripcion, prioridad, proyectoId);

        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Prioridad = prioridad;
        FechaLimite = fechaLimite;
        ProyectoId = proyectoId;

        Estado = EstadoTarea.Pendiente;
    }

    // Actualizar información de la tarea.
    public void Actualizar(
        string titulo,
        string descripcion,
        PrioridadTarea prioridad,
        DateTime fechaLimite)
    {
        ValidarDatos(titulo, descripcion, prioridad, ProyectoId);

        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Prioridad = prioridad;
        FechaLimite = fechaLimite;
    }

    // Cambiar estado de la tarea.
    public void CambiarEstado(EstadoTarea nuevoEstado)
    {
        if (!Enum.IsDefined(nuevoEstado))
            throw new ArgumentException("El estado indicado no es válido.");

        Estado = nuevoEstado;
    }

    // Comprobar si la tarea está vencida.
    public bool EstaVencida()
    {
        return FechaLimite < DateTime.Now &&
               Estado != EstadoTarea.Completada;
    }

    // Validaciones de la tarea.
    private static void ValidarDatos(
        string titulo,
        string descripcion,
        PrioridadTarea prioridad,
        Guid proyectoId)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException(
                "El título de la tarea es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripción de la tarea es obligatoria.");

        if (!Enum.IsDefined(prioridad))
            throw new ArgumentException(
                "La prioridad indicada no es válida.");

        if (proyectoId == Guid.Empty)
            throw new ArgumentException(
                "La tarea debe pertenecer a un proyecto.");
    }
}