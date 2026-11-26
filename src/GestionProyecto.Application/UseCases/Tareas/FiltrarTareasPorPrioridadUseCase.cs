using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Tareas;

public class FiltrarTareasPorPrioridadUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public FiltrarTareasPorPrioridadUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync(
        PrioridadTarea prioridad)
    {
        if (!Enum.IsDefined(prioridad))
            throw new ArgumentException("La prioridad indicada no es válida.");

        var tareas = await _tareaRepository.ObtenerPorPrioridadAsync(prioridad);

        return tareas.Select(t => t.ToDto()).ToList();
    }
}