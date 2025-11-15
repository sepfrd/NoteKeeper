using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Domain.Enums;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Models;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Infrastructure.Persistence;
using NoteKeeper.Shared.Constants;
using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion;

public class NotionOAuth2Service : INotionOAuth2Service
{
    private readonly UnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly IAuthService _authService;
    private readonly IMappingService _mappingService;
    private readonly NotionOAuthOptions _notionOAuthOptions;
    private readonly ILogger<NotionOAuth2Service> _logger;

    private readonly string _successMessage = string.Format(
        CultureInfo.InvariantCulture,
        SuccessMessages.OAuthTemplate,
        ExternalProviderConstants.Notion);

    private readonly string _failureMessage = string.Format(
        CultureInfo.InvariantCulture,
        ErrorMessages.OAuthTemplate,
        ExternalProviderConstants.Notion);

    public NotionOAuth2Service(
        UnitOfWork unitOfWork,
        HttpClient httpClient,
        IMemoryCache memoryCache,
        MemoryCacheEntryOptions memoryCacheEntryOptions,
        IAuthService authService,
        IMappingService mappingService,
        IOptions<AppOptions> appOptions,
        ILogger<NotionOAuth2Service> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = memoryCacheEntryOptions;
        _authService = authService;
        _notionOAuthOptions = appOptions.Value.NotionOAuthOptions;
        _logger = logger;
        _mappingService = mappingService;
    }

    public async Task<DomainResult<string?>> UseNotionOAuth2Async(CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            [userEntity => userEntity.ExternalProviderAccounts],
            cancellationToken);

        if (user is null)
        {
            return DomainResult<string?>.CreateFailure(ErrorMessages.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        if (user.ExternalProviderAccounts.All(account => account.ProviderName != ExternalProviderConstants.Notion))
        {
            return NotionGenerateOAuth2RequestUrl(user);
        }

        return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, _successMessage);
    }

    public async Task<DomainResult<string?>> NotionExchangeCodeForTokensAsync(
        NotionExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Clear();

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

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(nameof(AuthenticationSchemes.Basic), base64EncodedAuthenticationString);

        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return DomainResult<string?>.CreateFailure(_failureMessage, StatusCodes.Status500InternalServerError);
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var notionTokenResponseDto = JsonSerializer.Deserialize<NotionTokenResponseDto>(responseString);

        if (notionTokenResponseDto is null)
        {
            _logger.LogCritical(LogMessages.DeserializationErrorTemplate, typeof(NotionTokenResponseDto));

            return DomainResult<string?>.CreateFailure(_failureMessage, StatusCodes.Status500InternalServerError);
        }

        var isNotionTokenStored = await StoreNotionTokensAsync(
            notionTokenResponseDto,
            exchangeCodeForTokenRequestDto.State,
            cancellationToken);

        if (isNotionTokenStored)
        {
            return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, _successMessage);
        }

        _logger.LogCritical(LogMessages.DatabasePersistenceErrorTemplate, typeof(NotionToken), StringConstants.CreateActionName);

        return DomainResult<string?>.CreateFailure(_failureMessage, StatusCodes.Status500InternalServerError);
    }

    private DomainResult<string?> NotionGenerateOAuth2RequestUrl(User user)
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

        return DomainResult<string?>.CreateSuccess(null, StatusCodes.Status200OK, finalUrl);
    }

    private async Task<bool> StoreNotionTokensAsync(NotionTokenResponseDto notionTokenResponseDto, string state, CancellationToken cancellationToken = default)
    {
        var userId = RetrieveUserIdFromMemoryCache(state);

        if (userId is null)
        {
            return false;
        }

        var user = await _unitOfWork.UserRepository.GetByIdentityAsync(userId.Value, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var notionToken = _mappingService.Map<NotionTokenResponseDto, NotionToken>(notionTokenResponseDto);

        if (notionToken is null)
        {
            _logger.LogCritical(LogMessages.MappingErrorTemplate, typeof(NotionTokenResponseDto), typeof(NotionToken));

            return false;
        }

        await using var transaction = await _unitOfWork.Database.BeginTransactionAsync(cancellationToken);

        var commitSucceeded = false;

        try
        {
            notionToken.UserId = user.Id;

            await _unitOfWork.NotionTokenRepository.CreateAsync(notionToken, cancellationToken);

            user.ExternalProviderAccounts.Add(new ExternalProviderAccount
            {
                ProviderName = ExternalProviderConstants.Notion,
                ProviderType = ProviderType.OAuth,
                LinkedAt = DateTimeOffset.UtcNow,
                UserId = user.Id
            });

            _unitOfWork.UserRepository.Update(user);

            await _unitOfWork.CommitChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            commitSucceeded = true;

            return commitSucceeded;
        }
        catch (Exception exception)
        {
            _logger.LogError(LogMessages.DatabasePersistenceErrorTemplate, typeof(GoogleToken), StringConstants.CreateActionName);
            _logger.LogError(exception, LogMessages.ExceptionTemplate, exception.InnerException);

            await transaction.RollbackAsync(cancellationToken);

            return false;
        }
        finally
        {
            if (!commitSucceeded)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    private void StoreStateAndUserIdInMemoryCache(string state, long userId) =>
        _memoryCache.Set(state, userId, _memoryCacheEntryOptions);

    private long? RetrieveUserIdFromMemoryCache(string state) =>
        _memoryCache.TryGetValue<long>(state, out var userId) ? userId : null;
}