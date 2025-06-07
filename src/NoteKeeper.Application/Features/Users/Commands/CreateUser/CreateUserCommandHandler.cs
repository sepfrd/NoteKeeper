using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteKeeper.Application.Features.Users.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, DomainResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMappingService _mappingService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMappingService mappingService, ILogger<CreateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<DomainResult<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var isUsernameUnique = await _unitOfWork.UserRepository.IsUsernameUniqueAsync(command.Username, cancellationToken);

        if (!isUsernameUnique)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                ErrorMessages.PropertyNotUniqueTemplate,
                command.Username,
                nameof(User.Username).ToLowerInvariant());

            return DomainResult<UserDto>.CreateFailure(message, StatusCodes.Status400BadRequest);
        }

        var isEmailUnique = await _unitOfWork.UserRepository.IsEmailUniqueAsync(command.Email, cancellationToken);

        if (!isEmailUnique)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                ErrorMessages.PropertyNotUniqueTemplate,
                command.Email,
                nameof(User.Email).ToLowerInvariant());

            return DomainResult<UserDto>.CreateFailure(message, StatusCodes.Status400BadRequest);
        }

        var user = _mappingService.Map<CreateUserCommand, User>(command);

        if (user is null)
        {
            _logger.LogCritical(ErrorMessages.MappingLogTemplate, typeof(CreateUserCommand), typeof(User));

            return DomainResult<UserDto>.CreateFailure(
                ErrorMessages.InternalServerError,
                StatusCodes.Status500InternalServerError);
        }

        var createdUser = await _unitOfWork.UserRepository.CreateAsync(user, cancellationToken);

        await _unitOfWork.CommitChangesAsync(cancellationToken);

        var userDto = _mappingService.Map<User, UserDto>(createdUser);

        return DomainResult<UserDto>.CreateSuccess(null, StatusCodes.Status201Created, userDto!);
    }
}