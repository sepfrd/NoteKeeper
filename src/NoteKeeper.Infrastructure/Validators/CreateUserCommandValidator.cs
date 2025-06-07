using FluentValidation;
using NoteKeeper.Application.Features.Users.Commands.CreateUser;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(createUserRequestDto => createUserRequestDto.Email)
            .Matches(RegexPatternConstants.EmailRegexPattern)
            .WithMessage(ErrorMessages.EmailValidation);

        RuleFor(createUserRequestDto => createUserRequestDto.Username)
            .Matches(RegexPatternConstants.UsernameRegexPattern)
            .WithMessage(ErrorMessages.UsernameValidation);

        RuleFor(createUserRequestDto => createUserRequestDto.Password)
            .Matches(RegexPatternConstants.PasswordRegexPattern)
            .WithMessage(ErrorMessages.PasswordValidation);
    }
}