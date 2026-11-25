using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Tareas;

public class CambiarEstadoTareaUseCase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarEstadoTareaUseCase(
        ITareaRepository tareaRepository,
        IUnitOfWork unitOfWork)
    {
        _tareaRepository = tareaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EjecutarAsync(
        Guid id,
        EstadoTarea nuevoEstado,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar la tarea.
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        // 2. Comprobar si existe.
        if (tarea is null)
            return false;

        // 3. Cambiar el estado.
        tarea.CambiarEstado(nuevoEstado);

        // 4. Informar al repositorio.
        _tareaRepository.Actualizar(tarea);

        // 5. Guardar los cambios.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}