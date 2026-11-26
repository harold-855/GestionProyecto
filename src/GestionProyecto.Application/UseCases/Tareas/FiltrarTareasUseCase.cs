using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Tareas;

public class FiltrarTareasUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public FiltrarTareasUseCase(ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync(
        EstadoTarea? estado = null,
        PrioridadTarea? prioridad = null)
    {
        if (estado.HasValue && !Enum.IsDefined(estado.Value))
            throw new ArgumentException("El estado indicado no es válido.");

        if (prioridad.HasValue && !Enum.IsDefined(prioridad.Value))
            throw new ArgumentException("La prioridad indicada no es válida.");

        var tareas = await _tareaRepository.ObtenerPorFiltroAsync(estado, prioridad);

        return tareas.Select(t => t.ToDto()).ToList();
    }
}
