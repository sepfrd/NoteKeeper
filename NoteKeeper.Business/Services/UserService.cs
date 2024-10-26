using System.Globalization;
using System.Net;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.DomainEntities;
using NoteKeeper.Business.Dtos.Requests;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseDto<UserDto?>> CreateUserAsync(CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default)
    {
        var isUsernameUnique = await _userRepository.IsUsernameUniqueAsync(createUserRequestDto.Username, cancellationToken);

        if (!isUsernameUnique)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                MessageConstants.PropertyNotUniqueMessage,
                createUserRequestDto.Username,
                nameof(User.Username).ToLowerInvariant());

            return new ResponseDto<UserDto?>
            {
                IsSuccess = false,
                Message = message,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var isEmailUnique = await _userRepository.IsEmailUniqueAsync(createUserRequestDto.Email, cancellationToken);

        if (!isEmailUnique)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                MessageConstants.PropertyNotUniqueMessage,
                createUserRequestDto.Email,
                nameof(User.Email).ToLowerInvariant());

            return new ResponseDto<UserDto?>
            {
                IsSuccess = false,
                Message = message,
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }

        var user = createUserRequestDto.ToUserDomainEntity();

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = UserDto.FromUserDomainEntity(createdUser);

        return new ResponseDto<UserDto?>
        {
            Data = userDto,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.Created
        };
    }
}