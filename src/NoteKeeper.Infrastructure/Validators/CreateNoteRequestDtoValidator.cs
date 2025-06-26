using FluentValidation;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;

namespace NoteKeeper.Infrastructure.Validators;

public class CreateNoteRequestDtoValidator : AbstractValidator<CreateNoteRequestDto>
{
    public CreateNoteRequestDtoValidator()
    {
        RuleFor(requestDto => requestDto.Title).NotEmpty();
        RuleFor(requestDto => requestDto.Content).NotEmpty();
    }
}