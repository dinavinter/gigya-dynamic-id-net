using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSStore.GigyaApi
{

    public class GigyaModel
    {
        public readonly string ApiKey;
        public readonly string Domain;

        public GigyaModel(string apiKey, string domain = "gigya.com")
        {
            ApiKey = apiKey;
            Domain = domain;
        }

    }

    public class ErrorMessage
    {
        public string Message { get; set; }
        public string? RequestId { get; set; }
        public int? ErrorCode { get; set; }
        public object? Details { get; set; }

        public ErrorMessage(string message, string? requestId = null, int? errorCode = 0, object? details = null)
        {
            Message = message;
            RequestId = requestId;
            ErrorCode = errorCode;
            Details = details;
        }
    }

    public class GStatus
    {
        public int statusCode;
        public int errorCode;
        public string statusReason;
        public string callId;
        public string errorMessage;
        public string errorDetails;
        public string time;
        public DebugInfo debug;
        public IgnoredParam[] ignoredParams;
        public int apiVersion;
        public bool soa => apiVersion >= 2;

        [JsonIgnore]
        public string kibanaLink =>
            $"http://kibana/kibana3/#/dashboard/elasticsearch/SearchByCallID?my_url_parameter=callID:{callId}";

        public class IgnoredParam
        {
            public string paramName { get; set; }
            public string message { get; set; }
            public int warningCode { get; set; }
        }

        public class DebugInfo
        {
            public JsonElement exceptionDatas;

            public string exceptionMessage;

            public string offendingService;

            public string stackTrace;
        }

        public ErrorMessage ErrorMessage() => new ErrorMessage(errorMessage, callId, errorCode, this);
    }


    public interface ISearchResults<out TResult>
    {
        TResult[] Results { get; }

        int ObjectsCount { get; }


        int TotalCount { get; }

        string NextCursorId { get; }

        string CallId { get; }

        int ErrorCode { get; }

        string ErrorMessage { get; }
    }
}