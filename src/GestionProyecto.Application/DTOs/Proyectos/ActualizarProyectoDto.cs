namespace GestionProyecto.Application.DTOs.Proyectos;

public class ActualizarProyectoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFinalizacion { get; set; }
}