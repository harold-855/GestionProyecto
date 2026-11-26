using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class EliminarProyectoUseCase
{
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarProyectoUseCase(
        IProyectoRepository proyectoRepository,
        IUnitOfWork unitOfWork)
    {
        _proyectoRepository = proyectoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdConTareasAsync(id);

        if (proyecto is null)
            return false;

        if (proyecto.TieneTareasPendientes())
        {
            throw new BusinessRuleException(
                "No se puede eliminar un proyecto que tenga tareas pendientes.");
        }

        _proyectoRepository.Eliminar(proyecto);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}