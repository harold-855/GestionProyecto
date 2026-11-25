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
        // 1. Buscar el proyecto incluyendo sus tareas.
        var proyecto = await _proyectoRepository
            .ObtenerPorIdConTareasAsync(id);

        // 2. Comprobar si existe.
        if (proyecto is null)
            return false;

        // 3. Comprobar si tiene tareas pendientes.
        if (proyecto.TieneTareasPendientes())
        {
            throw new InvalidOperationException(
                "No se puede eliminar un proyecto que tenga tareas pendientes.");
        }

        // 4. Preparar la eliminación.
        _proyectoRepository.Eliminar(proyecto);

        // 5. Guardar los cambios.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}