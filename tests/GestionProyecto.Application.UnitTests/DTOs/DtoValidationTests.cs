using System.ComponentModel.DataAnnotations;
using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UnitTests.DTOs;

public class DtoValidationTests
{
    private static bool TryValidate(object instance, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
    }

    [Fact]
    public void CrearProyectoDto_ConLongitudesExcedidas_FallaValidacion()
    {
        var dto = new CrearProyectoDto
        {
            Nombre = new string('a', 151),
            Descripcion = new string('b', 501),
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(5)
        };

        var valido = TryValidate(dto, out var results);

        Assert.False(valido);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void CrearTareaDto_ConPrioridadInvalidaYLongitudesExcedidas_FallaValidacion()
    {
        var dto = new CrearTareaDto
        {
            Titulo = new string('t', 151),
            Descripcion = new string('d', 501),
            Prioridad = (PrioridadTarea)99,
            FechaLimite = DateTime.UtcNow.AddDays(3),
            ProyectoId = Guid.NewGuid()
        };

        var valido = TryValidate(dto, out var results);

        Assert.False(valido);
        Assert.Equal(3, results.Count);
    }
}
