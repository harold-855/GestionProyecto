using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class EliminarTareaUseCase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarTareaUseCase(
        ITareaRepository tareaRepository,
        IUnitOfWork unitOfWork)
    {
        _tareaRepository = tareaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        if (tarea is null)
            return false;

        _tareaRepository.Eliminar(tarea);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return true;
    }
}