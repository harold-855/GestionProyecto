using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareaPorIdUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareaPorIdUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<TareaDto?> EjecutarAsync(Guid id)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        return tarea?.ToDto();
    }
}