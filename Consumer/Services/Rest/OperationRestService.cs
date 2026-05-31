using Consumer.Common;
using Consumer.Common.Helpers;
using Consumer.Common.Interfaces;
using Consumer.Common.Models;
using Microsoft.Extensions.Options;

namespace Consumer.Services.Rest;

public class OperationRestService : IOperationService
{
    private readonly HttpClient client;
    
    public OperationRestService(IHttpClientFactory client, IOptions<IntegrationPlatformConfiguration> apiSettings)
    {
        this.client = client.CreateClient("integration-platform");
        this.client.BaseAddress = new Uri(apiSettings.Value.BaseUrl + "operations");
    }

    public async Task<Guid> AddOperation(AddAsyncOperationRequest request)
    {
        var content = JsonHelper.SerializeRequest(request);
        var result = await client.PostAsync(client.BaseAddress.ToString(), content);
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadAsStringAsync();
        return JsonHelper.DeserializeResult<Guid>(response);
    }
    
    public async Task<PaginatedListDto<GetOperationsResponseDto>> GetOperations(int pageNumber, int pageSize)
    {
        var result = await client.GetAsync($"{client.BaseAddress}?pageNumber={pageNumber}&pageSize={pageSize}");
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadAsStringAsync();
        return JsonHelper.DeserializeResult<PaginatedListDto<GetOperationsResponseDto>>(response);
    }
}