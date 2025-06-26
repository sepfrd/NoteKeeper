using FluentValidation;
using NoteKeeper.Application.Features.Users.Commands.CreateUser;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .Matches(RegexPatternConstants.EmailRegexPattern)
            .WithMessage(ErrorMessages.EmailValidation);

        RuleFor(command => command.Username)
            .Matches(RegexPatternConstants.UsernameRegexPattern)
            .WithMessage(ErrorMessages.UsernameValidation);

        RuleFor(command => command.Password)
            .Matches(RegexPatternConstants.PasswordRegexPattern)
            .WithMessage(ErrorMessages.PasswordValidation);
    }
}