using FluentValidation;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;

namespace NoteKeeper.Infrastructure.Validators;

public class UpdateNoteRequestDtoValidator : AbstractValidator<UpdateNoteRequestDto>
{
    public UpdateNoteRequestDtoValidator()
    {
        RuleFor(requestDto => requestDto.NewTitle).NotEmpty();
        RuleFor(requestDto => requestDto.NewContent).NotEmpty();
    }
}