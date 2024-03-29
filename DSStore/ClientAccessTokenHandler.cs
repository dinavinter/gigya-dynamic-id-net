using System.Net.Http.Headers;
using IdentityModel.AspNetCore.AccessTokenManagement;

namespace DSStore;

public class ClientAccessTokenHandler : DelegatingHandler
{
    private readonly IClientAccessTokenManagementService _accessTokenManagementService;
    private readonly string _tokenClientName;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="accessTokenManagementService">The Access Token Management Service</param>
    /// <param name="tokenClientName">The name of the token client configuration</param>
    public ClientAccessTokenHandler(IClientAccessTokenManagementService accessTokenManagementService, string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName)
    {
        _accessTokenManagementService = accessTokenManagementService;
        _tokenClientName = tokenClientName;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetTokenAsync(request, forceRenewal: false, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);


        // retry if 401
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            await SetTokenAsync(request, forceRenewal: true, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    /// <summary>
    /// Set an access token on the HTTP request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forceRenewal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task SetTokenAsync(HttpRequestMessage request, bool forceRenewal, CancellationToken cancellationToken)
    {
        var parameters = new ClientAccessTokenParameters
        {
            ForceRenewal = forceRenewal
        };

        var token = await _accessTokenManagementService.GetClientAccessTokenAsync(_tokenClientName, parameters, cancellationToken);
        await Console.Out.WriteLineAsync(token);
        if (!string.IsNullOrWhiteSpace(token))
        {
           request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);
        }
    }
}