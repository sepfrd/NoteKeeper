using NoteKeeper.Application.Common;

namespace NoteKeeper.Application.Interfaces;

public interface IDataValidator<in TEntity>
{
    DataValidationResult Validate(TEntity entity);

    Task<DataValidationResult> ValidateAsync(TEntity entity, CancellationToken cancellationToken = default);
}