using Consumer.Common.Helpers;
using Consumer.Common.Interfaces;
using Consumer.Common.Models;
using Microsoft.Extensions.Options;

namespace Consumer.Services.Rest;

public class SyncOperationRestService : ISyncOperationService
{
    private readonly HttpClient client;
    
    public SyncOperationRestService(IHttpClientFactory client, IOptions<IntegrationPlatformConfiguration> apiSettings)
    {
        this.client = client.CreateClient("integration-platform");
        this.client.BaseAddress = new Uri(apiSettings.Value.BaseUrl + "sync-operations");
    }

    public async Task AddSyncOperation(AddSyncOperationRequest request)
    {
        var content = JsonHelper.SerializeRequest(request);
        var result = await client.PostAsync(client.BaseAddress.ToString(), content);
        result.EnsureSuccessStatusCode();
    }
}