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
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        if (tarea is null)
            return false;

        tarea.CambiarEstado(nuevoEstado);

        _tareaRepository.Actualizar(tarea);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}