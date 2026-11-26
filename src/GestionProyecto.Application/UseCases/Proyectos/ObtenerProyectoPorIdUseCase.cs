using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ObtenerProyectoPorIdUseCase
{
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerProyectoPorIdUseCase(
        IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<ProyectoDto?> EjecutarAsync(Guid id)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdConTareasAsync(id);

        return proyecto?.ToDto();
    }
}