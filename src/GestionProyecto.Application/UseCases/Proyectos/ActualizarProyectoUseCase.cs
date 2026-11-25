using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ActualizarProyectoUseCase
{
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarProyectoUseCase(
        IProyectoRepository proyectoRepository,
        IUnitOfWork unitOfWork)
    {
        _proyectoRepository = proyectoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EjecutarAsync(
        Guid id,
        ActualizarProyectoDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar el proyecto.
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(id);

        // 2. Comprobar si existe.
        if (proyecto is null)
            return false;

        // 3. Actualizar sus propiedades.
        proyecto.Actualizar(
            dto.Nombre,
            dto.Descripcion,
            dto.FechaInicio,
            dto.FechaFinalizacion);

        // 4. Informar al repositorio que debe actualizarlo.
        _proyectoRepository.Actualizar(proyecto);

        // 5. Guardar los cambios.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}