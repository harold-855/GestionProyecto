using System.ComponentModel.DataAnnotations;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.DTOs.Tareas;

public class ActualizarTareaDto
{
    [Required(ErrorMessage = "El título de la tarea es obligatorio.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de la tarea es obligatoria.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La prioridad de la tarea es obligatoria.")]
    [EnumDataType(typeof(PrioridadTarea), ErrorMessage = "La prioridad indicada no es válida.")]
    public PrioridadTarea Prioridad { get; set; }

    [Required(ErrorMessage = "La fecha límite de la tarea es obligatoria.")]
    public DateTime FechaLimite { get; set; }
}
