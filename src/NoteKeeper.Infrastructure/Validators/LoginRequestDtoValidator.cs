using FluentValidation;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;
using NoteKeeper.Shared.Constants;

namespace NoteKeeper.Infrastructure.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(requestDto => requestDto.UsernameOrEmail)
            .Matches($"{RegexPatternConstants.UsernameRegexPattern}|{RegexPatternConstants.EmailRegexPattern}");

        RuleFor(requestDto => requestDto.Password)
            .Matches(RegexPatternConstants.PasswordRegexPattern);
    }
}