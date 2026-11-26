using System.ComponentModel.DataAnnotations;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.DTOs.Tareas;

public class CambiarEstadoTareaDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado indicado no es válido.")]
    public EstadoTarea Estado { get; set; }
}
