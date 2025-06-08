using FluentValidation;
using NoteKeeper.Application.Features.Users.Commands.CreateUser;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(createUserCommand => createUserCommand.Email)
            .Matches(RegexPatternConstants.EmailRegexPattern)
            .WithMessage(ErrorMessages.EmailValidation);

        RuleFor(createUserCommand => createUserCommand.Username)
            .Matches(RegexPatternConstants.UsernameRegexPattern)
            .WithMessage(ErrorMessages.UsernameValidation);

        RuleFor(createUserCommand => createUserCommand.Password)
            .Matches(RegexPatternConstants.PasswordRegexPattern)
            .WithMessage(ErrorMessages.PasswordValidation);
    }
}