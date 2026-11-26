using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Tareas;

public class FiltrarTareasPorEstadoUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public FiltrarTareasPorEstadoUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync(
        EstadoTarea estado)
    {
        if (!Enum.IsDefined(estado))
            throw new ArgumentException("El estado indicado no es válido.");

        var tareas = await _tareaRepository.ObtenerPorEstadoAsync(estado);

        return tareas.Select(t => t.ToDto()).ToList();
    }
}