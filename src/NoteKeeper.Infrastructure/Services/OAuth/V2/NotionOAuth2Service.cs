using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.Common.Dtos.Notion;

namespace NoteKeeper.Infrastructure.Services.OAuth.V2;

public class NotionOAuth2Service : INotionOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IRepositoryBase<NotionToken> _notionTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly NotionOAuthOptions _notionOAuthOptions;
    private readonly string _successMessage = string.Format(CultureInfo.InvariantCulture, MessageConstants.OAuthSuccessMessageTemplate, "Notion");
    private readonly string _failureMessage = string.Format(CultureInfo.InvariantCulture, MessageConstants.OAuthFailureMessageTemplate, "Notion");

    public NotionOAuth2Service(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IRepositoryBase<NotionToken> notionTokenRepository,
        IUserRepository userRepository,
        IAuthService authService,
        IOptions<AppOptions> appOptions)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _notionTokenRepository = notionTokenRepository;
        _userRepository = userRepository;
        _authService = authService;
        _notionOAuthOptions = appOptions.Value.NotionOAuthOptions;
    }

    public async Task<ResponseDto<string?>> UseNotionOAuth2Async(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.NotionToken),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        if (user.NotionToken is null)
        {
            return NotionGenerateOAuth2RequestUrl(user);
        }

        return new ResponseDto<string?>
        {
            IsSuccess = true,
            Message = _successMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    private ResponseDto<string?> NotionGenerateOAuth2RequestUrl(User user)
    {
        var state = Guid.NewGuid().ToString();

        StoreStateAndUserIdInMemoryCache(state, user.Id);

        IEnumerable<KeyValuePair<string, string?>> queryParameters =
        [
            new(CustomOAuthConstants.ClientIdParameterName, _notionOAuthOptions.ClientId),
            new(CustomOAuthConstants.ResponseTypeParameterName, CustomOAuthConstants.CodeResponseType),
            new(CustomOAuthConstants.NotionOwnerParameterName, CustomOAuthConstants.NotionUserOwnerType),
            new(CustomOAuthConstants.RedirectUriParameterName, _notionOAuthOptions.RedirectUri),
            new(CustomOAuthConstants.StateParameterName, state)
        ];

        var finalUrl = QueryHelpers.AddQueryString(_notionOAuthOptions.AuthUri, queryParameters);

        return new ResponseDto<string?>
        {
            Data = finalUrl,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<string?>> NotionExchangeCodeForTokensAsync(
        NotionExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(5d);
        httpClient.DefaultRequestHeaders.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, _notionOAuthOptions.TokenUri);

        var requestBody = new NotionTokenRequestBodyDto
        {
            GrantType = CustomOAuthConstants.AuthorizationCodeGrantType,
            Code = exchangeCodeForTokenRequestDto.Code,
            RedirectUri = _notionOAuthOptions.RedirectUri
        };

        var jsonMediaType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

        var content = new StringContent(JsonSerializer.Serialize(requestBody), mediaType: jsonMediaType);

        var authenticationString = $"{_notionOAuthOptions.ClientId}:{_notionOAuthOptions.ClientSecret}";

        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Basic.ToString(), base64EncodedAuthenticationString);

        request.Content = content;

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = _failureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var notionTokenResponseDto = JsonSerializer.Deserialize<NotionTokenResponseDto>(responseString);

        if (notionTokenResponseDto is null)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = _failureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        var isNotionTokenStored = await StoreNotionTokensAsync(
            notionTokenResponseDto,
            exchangeCodeForTokenRequestDto.State,
            cancellationToken);

        if (!isNotionTokenStored)
        {
            return new ResponseDto<string?>
            {
                IsSuccess = false,
                Message = _failureMessage,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }

        return new ResponseDto<string?>
        {
            IsSuccess = true,
            Message = _successMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    private async Task<bool> StoreNotionTokensAsync(NotionTokenResponseDto notionTokenResponseDto, string state, CancellationToken cancellationToken = default)
    {
        var userId = RetrieveUserIdFromMemoryCache(state);

        if (userId is null)
        {
            return false;
        }

        var user = await _userRepository
            .GetByIdAsync(
                userId.Value,
                users => users.Include(user => user.NotionToken),
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        var notionToken = notionTokenResponseDto.ToNotionTokenDomainEntity();

        notionToken.UserId = user.Id;

        await _notionTokenRepository.CreateAsync(notionToken, cancellationToken);

        user.MarkAsUpdated();

        var insertCount = await _notionTokenRepository.SaveChangesAsync(cancellationToken);

        return insertCount > 0;
    }

    private void StoreStateAndUserIdInMemoryCache(string state, long userId) =>
        _memoryCache.Set(state, userId, _memoryCacheEntryOptions);

    private long? RetrieveUserIdFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<long>(state, out var userId) ? userId : null;
}