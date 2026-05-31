using Consumer.Services.Rest;
using Microsoft.Extensions.Options;

namespace Consumer.Services;

public static class ConfigureServices
{
    public static void AddIntegrationPlatformHttpClients(this IServiceCollection services)
    {
        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<IntegrationPlatformConfiguration>>().Value;
        services.AddHttpClient("integration-platform", (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(15)
                };
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
    }
}