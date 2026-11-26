using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ActualizarTareaUseCase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarTareaUseCase(
        ITareaRepository tareaRepository,
        IUnitOfWork unitOfWork)
    {
        _tareaRepository = tareaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EjecutarAsync(
        Guid id,
        ActualizarTareaDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar la tarea.
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        // 2. Comprobar si existe.
        if (tarea is null)
            return false;

        // 3. Actualizar sus propiedades.
        tarea.Actualizar(
            dto.Titulo,
            dto.Descripcion,
            dto.Prioridad,
            dto.FechaLimite);

        // 4. Registrar la actualización.
        _tareaRepository.Actualizar(tarea);

        // 5. Guardar los cambios.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}