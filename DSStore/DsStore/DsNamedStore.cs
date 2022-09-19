using System.Text.Json;
using AspNetCoreDemoApp.DsStore;
using DSStore.GigyaApi;
using Microsoft.Extensions.Options;

namespace DSStore;

    public class DsStore : IStore
    {
        private readonly HttpJsonClient _httpJsonClient;
        private readonly Namespace _ds;

        public DsStore(HttpJsonClient httpJsonClient, Api api)
        {
            _httpJsonClient = httpJsonClient;
            _ds = api.ds;
        }

        public async Task SaveAsync(string type, string id, object entity,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var response = await _httpJsonClient.Send(_ds.GetRequest(new DsStoreRequest(id, type, entity)),
                cancellationToken);
            EnsureSuccessResponse(response!);
        }


        public async Task DeleteAsync(string type, string id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var response = await _httpJsonClient.Send(_ds.GetRequest(new DsDeleteRequest(id, type)),
                cancellationToken);
            EnsureSuccessResponse(response!);
        }


        public async Task<(string[] deleted, string[] failed, string[] canceled)> DeleteManyAsync(string type,
            string? filter,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var items = await FindAsync(type, "oid", filter, cancellationToken: cancellationToken);
            List<string> deleted = new List<string>();
            List<string> failed = new List<string>();
            Stack<string> toDelete = new Stack<string>(items.Select(e => e.Id));

            while (toDelete.Any() && !cancellationToken.IsCancellationRequested)
            {
                var id = toDelete.Pop();
                try
                {
                    await DeleteAsync(type, id, cancellationToken: cancellationToken);
                    deleted.Add(id);
                }
                catch (Exception e)
                {
                    failed.Add(id);
                }
            }

            return (deleted.ToArray(), failed.ToArray(), toDelete.ToArray());
        }


        public async Task<IEnumerable<DsEntity>> FindAsync(string type, string? select = null, string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var values = await FindAsync<DsEntity>(type, select, filter, orderBy, cancellationToken);
            return values!.ToList();
        }

        public async Task<IEnumerable<JsonDocument>> FindJsonAsync(string type, string? select = null,
            string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var values = await FindAsync<JsonDocument>(type, select, filter, orderBy, cancellationToken);
            return values.ToList();
        }

        public async Task<int> CountAsync(string type, string? filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var query = Query("count(*)", filter);

            var response = await _httpJsonClient.Get<SearchResponse>(
                _ds.GetRequest(new DsSearchRequest(query)), cancellationToken);
            EnsureSuccessResponse(response!);
            return (int) response!.SelectMetric("count(*)");
        }


        public async Task<IEnumerable<TResult>> FindAsync<TResult>(string type, string? select, string? filter,
            string? orderBy = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var query = Query(type, @select, filter, orderBy);
            var response =
                await _httpJsonClient.Get<SearchResponse>(
                    _ds.GetRequest(new DsSearchRequest(query)), cancellationToken);
            EnsureSuccessResponse(response);
            var values = response!.SelectResults<TResult>();
            return values;
        }

        private string Query(string type, string? @select = null, string? filter = null, string? orderBy = null)
        {
            var whereClause = filter != null ? $"where {filter}" : "";
            var orderByClause = orderBy != null ? $"orderBy {orderBy}" : "";
            var selectClause = @select != null ? $"select {@select}" : "select * ";
            var query = $"{selectClause} from {type} {whereClause} {orderByClause}";
            return query;
        }

        private static void EnsureSuccessResponse(GStatus response)
        {
            if (response.errorCode > 0)
            {
                throw new Exception($"Error {response.errorCode} {response.errorMessage} {response.errorDetails}  ");
            }
        }
    }

public class DsStore<T> : IStore<T> where T : IEntity
{
    private readonly DsStore _dsStore;
    private readonly string _type = typeof(T).Name.ToLower();

    public DsStore(DsStore dsStore)
    {
        _dsStore = dsStore;
    }

    public Task SaveAsync(T entity, CancellationToken cancellationToken = new CancellationToken())
    {
        return _dsStore.SaveAsync(_type, entity.Id, entity, cancellationToken);
    }


    public Task<IEnumerable<T>> FindAsync(string? select, string? filter, string? orderBy = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _dsStore.FindAsync<T>(_type, select, filter, orderBy, cancellationToken);
    }


    public Task UseDs(Func<string, IStore, Task> action)
    {
        return action(_type, _dsStore);
    }
}



public interface IStore<T>
{
}

