using System.Net.Http.Json;
using System.Text.Json;
using AspNetCoreDemoApp.DsStore;
using DSStore.GigyaApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSStore
{
    public interface IStoreFactory
    {
        DsTypedStore GetStore(string type);

    }


    public class StoreFactory
    {

    }

    public interface IDsTypedStore
    {
        Task SaveAsync(string id, object entity,
            CancellationToken cancellationToken = new CancellationToken());

        Task<DsEntity?> GetAsync(string id,
            CancellationToken cancellationToken = new CancellationToken());

        Task DeleteAsync(string id,
            CancellationToken cancellationToken = new CancellationToken());

        Task<(string[] deleted, string[] failed, string[] canceled)> DeleteManyAsync(
            string? filter,
            CancellationToken cancellationToken = new CancellationToken());

        Task<IEnumerable<DsEntity>> FindAsync(string? select = null, string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<IEnumerable<TResult>> FindAsync<TResult>(string? select, string? filter,
            string? orderBy = null, CancellationToken cancellationToken = new CancellationToken());

        Task<IEnumerable<JsonDocument>> FindJsonAsync(string? select = null,
            string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken());

        Task<int> CountAsync(string? filter = null,
            CancellationToken cancellationToken = new CancellationToken());
    }

    public class DsTypedStore : IStore, IDsTypedStore
    {
        private readonly string _type;
        private readonly HttpClient _httpClient;

        public DsTypedStore(IOptions<DsOptions> dsOptions, IOptions<GigyaOptions> gigyaOptions,  HttpClient httpClient)
        {
            _httpClient = httpClient;
            _type = dsOptions.Value.Type;

            Console.Out.WriteLine("BaseAddress: " + httpClient.BaseAddress);
            Console.Out.WriteLine("Type: " + _type);
        }

        public async Task SaveAsync(string id, object entity,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var response = await _httpClient.GetFromJsonAsync<GStatus?>(new DsStoreRequest(id, _type, entity).Uri(),
                cancellationToken: cancellationToken);
            EnsureSuccessResponse(response!);
        }

        public async Task<DsEntity?> GetAsync(string id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await _httpClient.GetFromJsonAsync<DsEntity?>(new DsGetRequest(id, _type).Uri(),
                cancellationToken: cancellationToken);

        }


        public async Task DeleteAsync(string id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = new DsDeleteRequest(id, _type).Uri();
            await Console.Out.WriteLineAsync("Path: " +uri);
            await Console.Out.WriteLineAsync("Base: " +_httpClient.BaseAddress);

            var response = await _httpClient.GetFromJsonAsync<GStatus?>(new DsDeleteRequest(id, _type).Uri(),
                cancellationToken: cancellationToken);
            EnsureSuccessResponse(response!);
        }


        public async Task<(string[] deleted, string[] failed, string[] canceled)> DeleteManyAsync(
            string? filter,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var items = await FindAsync(_type, "oid", filter, cancellationToken: cancellationToken);
            List<string> deleted = new List<string>();
            List<string> failed = new List<string>();
            Stack<string> toDelete = new Stack<string>(items.Select(e => e.Id));

            while (toDelete.Any() && !cancellationToken.IsCancellationRequested)
            {
                var id = toDelete.Pop();
                try
                {
                    await DeleteAsync(id, cancellationToken: cancellationToken);
                    deleted.Add(id);
                }
                catch (Exception e)
                {
                    failed.Add(id);
                }
            }

            return (deleted.ToArray(), failed.ToArray(), toDelete.ToArray());
        }


        public async Task<IEnumerable<DsEntity>> FindAsync(string? select = null, string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var values = await FindAsync<DsEntity>(select, filter, orderBy, cancellationToken);
            return values!.ToList();
        }

        public async Task<IEnumerable<JsonDocument>> FindJsonAsync(string? select = null,
            string? filter = null,
            string? orderBy = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var values = await FindAsync<JsonDocument>(select, filter, orderBy, cancellationToken);
            return values.ToList();
        }

        public async Task<int> CountAsync(string? filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var query = Query("count(*)", filter);
            var response = await _httpClient.GetFromJsonAsync<SearchResponse?>(new DsSearchRequest(  query).Uri(),
                cancellationToken: cancellationToken);
            EnsureSuccessResponse(response!);
            return (int) response!.SelectMetric("count(*)");
        }


        public async Task<IEnumerable<TResult>> FindAsync<TResult>(string? select, string? filter,
            string? orderBy = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var query = Query(@select, filter, orderBy);
            var response = await _httpClient.GetFromJsonAsync<SearchResponse?>(new DsSearchRequest(  query).Uri(),
                cancellationToken: cancellationToken);
            EnsureSuccessResponse(response);
            var values = response!.SelectResults<TResult>();
            return values;
        }

        private string Query(string? @select = null, string? filter = null, string? orderBy = null)
        {
            var whereClause = filter != null ? $"where {filter}" : "";
            var orderByClause = orderBy != null ? $"orderBy {orderBy}" : "";
            var selectClause = @select != null ? $"select {@select}" : "select * ";
            var query = $"{selectClause} from {_type} {whereClause} {orderByClause}";
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




    public interface IStore
    {
    }
}