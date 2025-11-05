using SunsetCars.Data.Repositories;

namespace SunsetCars.Services.Infrastructure;

public interface IProtocolGenerator
{
    Task<string> GenerateAsync(string prefix = "SC");
    string Generate(string prefix = "SC");
}

public class ProtocolGeneratorService : IProtocolGenerator
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<ProtocolGeneratorService> _logger;

    public ProtocolGeneratorService(
        ISaleRepository saleRepository,
        ILogger<ProtocolGeneratorService> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prefix = "SC")
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var protocol = Generate(prefix);

            if (!await _saleRepository.ProtocolExistsAsync(protocol))
            {
                _logger.LogDebug("Protocol generated: {Protocol} on attempt {Attempt}", protocol, attempt + 1);
                return protocol;
            }

            _logger.LogDebug("Protocol {Protocol} already exists, retrying...", protocol);
        }

        // Fallback com timestamp mais preciso e GUID
        var fallbackProtocol = $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
        _logger.LogWarning("Using fallback protocol after {MaxAttempts} attempts: {Protocol}", maxAttempts, fallbackProtocol);
        return fallbackProtocol;
    }

    public string Generate(string prefix = "SC")
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(1000, 9999); // Thread-safe no .NET 6+
        return $"{prefix}-{date}-{random}";
    }
}
