using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ObtenerProyectosUseCase
{
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerProyectosUseCase(
        IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IReadOnlyList<ProyectoDto>> EjecutarAsync()
    {
        var proyectos = await _proyectoRepository.ObtenerTodosAsync();

        return proyectos.Select(p => p.ToDto()).ToList();
    }
}