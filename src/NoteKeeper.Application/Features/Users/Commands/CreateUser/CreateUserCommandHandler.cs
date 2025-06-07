using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
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

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMappingService mappingService)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
    }

    public async Task<DomainResult<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var isUsernameUnique = await _unitOfWork.IsUsernameUniqueAsync(command.Username, cancellationToken);

        if (!isUsernameUnique)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                ErrorMessages.PropertyNotUniqueTemplate,
                command.Username,
                nameof(User.Username).ToLowerInvariant());

            return DomainResult<UserDto>.CreateFailure(message, StatusCodes.Status400BadRequest);
        }

        var isEmailUnique = await _unitOfWork.IsEmailUniqueAsync(command.Email, cancellationToken);

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

        await _unitOfWork.CreateAsync(user!, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = UserDto.FromUserDomainEntity(createdUser);

        return new ResponseDto<UserDto?>
        {
            Data = userDto,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.Created
        };
    }
}