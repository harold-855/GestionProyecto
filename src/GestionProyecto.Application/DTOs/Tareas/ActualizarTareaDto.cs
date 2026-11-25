using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.DTOs.Tareas;

public class ActualizarTareaDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public PrioridadTarea Prioridad { get; set; }

    public DateTime FechaLimite { get; set; }
}