using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSStore;
public interface IEntity
{
    string Id { get; set; }
    string Uid { get;set;  }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Document { get; set; }

    public string ToJson()=>JsonSerializer.Serialize(this);

}
public class DsEntity : IEntity
{
    [JsonPropertyName("oid")] public string Id { get; set; }

    [JsonPropertyName("uid")] public string Uid { get; set; }


    [JsonExtensionData] public Dictionary<string, JsonElement>? Document { get; set; }

    public T? Get<T>(JsonSerializerOptions? serializerOptions = null)
    {
        var stringValue = JsonSerializer.Serialize(this, SerializerOptions ?? serializerOptions);
        return JsonSerializer.Deserialize<T>(stringValue, SerializerOptions ?? serializerOptions);
    }

    private static readonly JsonSerializerOptions? SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IgnoreNullValues = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}