using System.ComponentModel.DataAnnotations;

namespace GestionProyecto.Application.DTOs.Proyectos;

public class CrearProyectoDto
{
    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del proyecto es obligatoria.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    public DateTime FechaFinalizacion { get; set; }
}
