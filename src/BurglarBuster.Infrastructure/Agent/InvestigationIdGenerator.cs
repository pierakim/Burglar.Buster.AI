namespace BurglarBuster.Infrastructure.Agent;

internal static class InvestigationIdGenerator
{
    public static string NewId(TimeProvider timeProvider)
    {
        var timestamp = timeProvider.GetUtcNow().ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"INV-{timestamp}-{random}";
    }
}
