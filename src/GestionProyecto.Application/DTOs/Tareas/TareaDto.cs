using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.DTOs.Tareas;

public class TareaDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public EstadoTarea Estado { get; set; }

    public PrioridadTarea Prioridad { get; set; }

    public DateTime FechaLimite { get; set; }

    public Guid ProyectoId { get; set; }

    public bool EstaVencida { get; set; }
}