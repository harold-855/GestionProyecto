namespace GestionProyecto.Application.DTOs.Proyectos;

public class AvanceProyectoDto
{
    public Guid ProyectoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int TotalTareas { get; set; }

    public int TareasCompletadas { get; set; }

    public decimal PorcentajeAvance { get; set; }
}
