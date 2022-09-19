using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using DSStore.GigyaApi;

namespace AspNetCoreDemoApp.DsStore
{
    public class SearchResponse :  GStatus
    {
        public string CallId { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public DateTime Time { get; set; }

        public JsonElement[] Results { get; set; }

        public int ObjectsCount { get; set; }

        public int TotalCount { get; set; }

        public string NextCursorId { get; set; }

        public IEnumerable<T?> SelectResults<T>()
        {
            return Results.Select(jsonElement =>
                        jsonElement.EnumerateObject().FirstOrDefault(e=>e.Name == "data")
                            .Value.GetRawText())
                    .Select(e=>   JsonSerializer.Deserialize<T>(e, SerializerOptions))
                ;
            // .Select(e=>e.data!);
        }


        public double SelectMetric (string metric)
        {
            return Results.Select(jsonElement =>
                {
                    jsonElement.TryGetProperty(metric, out var element);
                    return element;
                }).Select(e => e.GetDouble())
                .First();

        }

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

    }
}