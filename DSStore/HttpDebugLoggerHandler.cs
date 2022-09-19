using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DSStore;

public class HttpDebugLoggerHandler : DelegatingHandler
{
    private readonly
#nullable disable
        ILogger<HttpDebugLoggerHandler> _logger;

    public HttpDebugLoggerHandler(ILogger<HttpDebugLoggerHandler> logger) => this._logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage httpResponseMessage1;
        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            this._logger.LogInformation(string.Format("HTTP {0} {1} Starting request", (object) request.Method, (object) request.RequestUri));
            HttpResponseMessage httpResponseMessage2 = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            this._logger.LogInformation(string.Format("HTTP {0} {1} Finished request within {2}ms", (object) request.Method, (object) request.RequestUri, (object) sw.ElapsedMilliseconds));
            httpResponseMessage1 = httpResponseMessage2;
        }
        catch (Exception ex)
        {
            this._logger.LogError((EventId) 500, "http.errors" + ex.Message + "\r\nspan start time: ", (object) ex);
            throw;
        }
        return httpResponseMessage1;
    }
}