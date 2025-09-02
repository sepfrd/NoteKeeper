using System.Threading.RateLimiting;

namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations.RateLimiters;

public class CustomFixedWindowRateLimiterOptions
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public double WindowSeconds { get; set; }

    public bool AutoReplenishment { get; set; }

    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}