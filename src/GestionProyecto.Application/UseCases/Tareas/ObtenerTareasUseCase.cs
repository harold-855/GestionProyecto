using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareasUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareasUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync()
    {
        var tareas = await _tareaRepository.ObtenerTodasAsync();

        return tareas.Select(t => t.ToDto()).ToList();
    }
}