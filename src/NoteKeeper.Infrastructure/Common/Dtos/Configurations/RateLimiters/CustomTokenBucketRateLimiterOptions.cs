using System.Threading.RateLimiting;

namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations.RateLimiters;

public class CustomTokenBucketRateLimiterOptions
{
    public int TokenLimit { get; set; }

    public int TokensPerPeriod { get; set; }

    public int QueueLimit { get; set; }

    public double ReplenishmentPeriodSeconds { get; set; }

    public bool AutoReplenishment { get; set; }

    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}