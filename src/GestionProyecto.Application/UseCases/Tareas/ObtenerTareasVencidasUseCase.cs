using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareasVencidasUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareasVencidasUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync()
    {
        var tareas = await _tareaRepository.ObtenerVencidasAsync();

        return tareas.Select(t => t.ToDto()).ToList();
    }
}