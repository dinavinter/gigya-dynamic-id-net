using System.Text.Json;

namespace DSStore.GigyaApi;

public class DsStoreRequest : Request
{
    public DsStoreRequest(string oid, string type, object data) : base("ds.store",
        new Dictionary<string, string>
        {
            [nameof(oid)] = oid, [nameof(type)] = type,
            [nameof(data)] = JsonSerializer.Serialize(data)
        })
    {
    }
}

public class DsGetRequest : Request
{
    public DsGetRequest(string oid, string type) : base("ds.get",
        new Dictionary<string, string> {[nameof(oid)] = oid, [nameof(type)] = type})
    {
    }
}

public class DsDeleteRequest : Request
{
    public DsDeleteRequest(string oid, string type) : base("ds.delete",
        new Dictionary<string, string> {[nameof(oid)] = oid, [nameof(type)] = type})
    {
    }
}

public class DsSearchRequest : Request
{
    public DsSearchRequest(string query) : base("ds.search",
        new Dictionary<string, string>
        {
            [nameof(query)] = query
        })
    {
    }
}