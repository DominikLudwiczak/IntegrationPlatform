using System.Text.Json;

namespace Consumer.Common.Helpers;

public static class JsonHelper
{
    public static T? DeserializeResult<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    
    public static StringContent SerializeRequest<T>(T request)
    {
        var json = JsonSerializer.Serialize(request);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}