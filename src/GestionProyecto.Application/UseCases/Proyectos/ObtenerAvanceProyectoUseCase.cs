using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ObtenerAvanceProyectoUseCase
{
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerAvanceProyectoUseCase(
        IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<AvanceProyectoDto?> EjecutarAsync(Guid id)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdConTareasAsync(id);

        return proyecto?.ToAvanceDto();
    }
}
