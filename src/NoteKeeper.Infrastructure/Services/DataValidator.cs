using FluentValidation;
using NoteKeeper.Application.Common;
using NoteKeeper.Application.Interfaces;

namespace NoteKeeper.Infrastructure.Services;

public class DataValidator<TEntity> : IDataValidator<TEntity>
{
    private readonly IValidator<TEntity> _validator;

    public DataValidator(IValidator<TEntity> validator)
    {
        _validator = validator;
    }

    public DataValidationResult Validate(TEntity entity)
    {
        var validationResult = _validator.Validate(entity);

        return new DataValidationResult(validationResult.IsValid, validationResult.ToString().Split(Environment.NewLine));
    }

    public async Task<DataValidationResult> ValidateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(entity, cancellationToken);

        return new DataValidationResult(validationResult.IsValid, validationResult.ToString().Split(Environment.NewLine));
    }
}