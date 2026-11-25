namespace GestionProyecto.Application.DTOs.Proyectos;

public class ProyectoDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFinalizacion { get; set; }

    public decimal PorcentajeAvance { get; set; }

    public int TotalTareas { get; set; }

    public int TareasCompletadas { get; set; }
}