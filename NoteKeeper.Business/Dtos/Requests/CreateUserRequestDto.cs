using System.ComponentModel.DataAnnotations;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Utilities;
using NoteKeeper.DataAccess.Entities;

namespace NoteKeeper.Business.Dtos.Requests;

public record CreateUserRequestDto
{
    [RegularExpression(
        RegexPatternConstants.UsernameRegexPattern,
        ErrorMessage = MessageConstants.UsernameValidationErrorMessage)]
    public required string Username { get; init; }

    [RegularExpression(
        RegexPatternConstants.PasswordRegexPattern,
        ErrorMessage = MessageConstants.PasswordValidationErrorMessage)]
    public required string Password { get; init; }

    [RegularExpression(
        RegexPatternConstants.EmailRegexPattern,
        ErrorMessage = MessageConstants.EmailValidationErrorMessage)]
    public required string Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public User ToUserDomainEntity() => new()
    {
        Username = Username.ToLowerInvariant(),
        Email = Email.ToLowerInvariant(),
        PasswordHash = CryptographyHelper.HashPassword(Password),
        FirstName = FirstName,
        LastName = LastName
    };
}