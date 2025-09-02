using System.Threading.RateLimiting;

namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations.RateLimiters;

public class CustomRateLimitersOptions
{
    public CustomFixedWindowRateLimiterOptions? FixedWindowRateLimiterOptions { get; set; }

    public CustomTokenBucketRateLimiterOptions? TokenBucketRateLimiterOptions { get; set; }

    public ConcurrencyLimiterOptions? ConcurrencyLimiterOptions { get; set; }
}