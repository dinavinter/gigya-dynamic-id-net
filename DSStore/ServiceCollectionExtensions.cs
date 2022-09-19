using System.Text.Json;
using System.Text.Json.Serialization;
using DSStore.GigyaApi;
using Http.Client.Options.Tracing;
using Http.Options;
using Http.Options.Standalone;
using Http.Options.Tracing.OpenTelemetry;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Orleans.Runtime;

namespace DSStore;

public static class ServiceCollectionExtensions
{
    public static void AddDsStore(this IServiceCollection serviceCollection, string name,
        Action<DsOptions>? configureDs = null, Action<GigyaOptions>? configureGigya = null)
    {
        serviceCollection.AddDsStore(configureDs, configureGigya);
    }

    public static IDsTypedStore GetDsStore(this IServiceProvider serviceProvider, string name)
    {
        return serviceProvider.GetRequiredService<IOptionsMonitor<StoreOptions>>().Get(name).Store;
    }

    public static HttpClientOptions AddClientAccessTokenHandler(this HttpClientOptions httpClientOptions,
        string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName)
    {
        httpClientOptions.AddHandler<ClientAccessTokenHandler>((provider, options) =>
        {
            var accessTokenManagementService = provider.GetRequiredService<IClientAccessTokenManagementService>();

            return new ClientAccessTokenHandler(accessTokenManagementService, tokenClientName);
        });
        httpClientOptions.AddHandler<HttpDebugLoggerHandler>((sp,options) => new HttpDebugLoggerHandler(sp.GetRequiredService<ILogger<HttpDebugLoggerHandler>>()));
        return httpClientOptions;
    }




    public class GigyaAPIHttpClientConfigurator : IConfigureNamedOptions<HttpClientOptions>
    {
        private readonly AccessTokenManagementOptions _accessTokenManagementOptions;
        private readonly IOptionsMonitor<GigyaOptions> _gigyaOptions;

        public GigyaAPIHttpClientConfigurator(AccessTokenManagementOptions accessTokenManagementOptions,
            IOptionsMonitor<GigyaOptions> gigyaOptions)
        {
            _accessTokenManagementOptions = accessTokenManagementOptions;
            _gigyaOptions = gigyaOptions;
        }

        public void Configure(string name, HttpClientOptions options)
        {
            var op = _gigyaOptions.Get(name);

            _accessTokenManagementOptions.Client.Clients.Add($"{name}-idp", new ClientCredentialsTokenRequest
            {
                Address = op.OAuthSettings.Token,
                ClientId = op.OAuthSettings.ClientId,
                ClientSecret = op.OAuthSettings.ClientSecret,
                Scope = "api",
                GrantType = "none"
            });

            options.AddClientAccessTokenHandler($"{name}-idp");
            options.Connection.Server = _gigyaOptions.Get(name).Api.accounts.Domain();
            Console.Out.WriteLine("name: " + name);
            Console.Out.WriteLine("domain: " + _gigyaOptions.Get(name).Api.ds.Domain());
            Console.Out.WriteLine("api: " + _gigyaOptions.Get(name).Site.ApiKey);
        }

        public void Configure(HttpClientOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }

    public class GigyaDsAPIHttpClientConfigurator : IConfigureNamedOptions<HttpClientOptions>
    {
        private readonly IOptionsMonitor<GigyaOptions> _optionsMonitor;

        public GigyaDsAPIHttpClientConfigurator(IOptionsMonitor<GigyaOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public void Configure(string name, HttpClientOptions options)
        {
            options.Connection.Server = _optionsMonitor.Get(name).Api.ds.Domain();
            Console.Out.WriteLine("name: " + name);
            Console.Out.WriteLine("domain: " + _optionsMonitor.Get(name).Api.ds.Domain());
            Console.Out.WriteLine("api: " + _optionsMonitor.Get(name).Site.ApiKey);
        }

        public void Configure(HttpClientOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }

    public class DsOptionsOptionsConfigure : IConfigureNamedOptions<StoreOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public DsOptionsOptionsConfigure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Configure(StoreOptions options)
        {
            Configure(Options.DefaultName, options);
        }

        public void Configure(string name, StoreOptions options)
        {
            options.Store = DsStorageFactory.Create(_serviceProvider, name);
        }
    }

    public static void AddDsStore(this IServiceCollection serviceCollection, Action<DsOptions>? configureDs = null,
        Action<GigyaOptions>? configureGigya = null)
    {
        serviceCollection.AddScoped<GigyaAPIHttpClientConfigurator>();
        serviceCollection.AddScoped<GigyaDsAPIHttpClientConfigurator>();

        serviceCollection.AddOptions<DsOptions>()
            .Configure(configureDs ?? (_ => { }));
        serviceCollection.ConfigureOptions<DsOptionsOptionsConfigure>();

        serviceCollection.AddOptions<GigyaOptions>()
            .Configure(configureGigya ?? (_ => { }));
        serviceCollection.ConfigureAll<GigyaOptions>(options =>
        {
            options.Site = GigyaSite.Test;
            options.Creds = GigyaCreds.Test;
            ;
        });
        // serviceCollection.AddClientAccessTokenManagement();
        serviceCollection.AddOptions<StoreOptions>();
//         var name = "ds";
// serviceCollection.AddAccessTokenManagement((services, clientAccessOptions) =>
// {
//               var gigyaOptions = services.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
//             clientAccessOptions.Client.Clients.Add($"{name}-idp", new ClientCredentialsTokenRequest
//             {
//                 Address = gigyaOptions.Get(name).OAuthSettings.Token,
//                 ClientId = gigyaOptions.Get(name).OAuthSettings.ClientId,
//                 ClientSecret = gigyaOptions.Get(name).OAuthSettings.ClientSecret,
//                 Scope = "api",
//                 GrantType = "none"
//             });
//
//             Console.Out.WriteLine("idp name: " + name);
//             Console.Out.WriteLine("token: " + gigyaOptions.Get(name).OAuthSettings.Token);
//         });
//
// serviceCollection.AddHttpClient(name)
//     .ConfigureHttpClient((sp, client) =>
//     {
//         var gigyaOptions = sp.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
//
//         client.BaseAddress= new Uri(gigyaOptions.Get(name).Api.ds.Domain());
//
//     } )                .AddClientAccessTokenHandler($"{name}-idp");


// serviceCollection.AddHttpClientOptions((serviceName, clientOptions) =>
// {
//     Console.Out.WriteLine("serviceName: " + serviceName);
//
//     if (serviceName != $"{name}-idp")
//     {
//         clientOptions.ServiceName = serviceName;
//         clientOptions.Connection.Server = gigyaOptions.Get(serviceName).Api.ds.Domain();
//         clientOptions.Connection.Schema = "https";
//         clientOptions.AddClientAccessTokenHandler($"{name}-idp");
//         Console.Out.WriteLine("name: " + name);
//         Console.Out.WriteLine("domain: " + gigyaOptions.Get(name).Api.ds.Domain());
//         Console.Out.WriteLine("api: " + gigyaOptions.Get(name).Site.ApiKey);
//     }
// });
        var name = "ds";
        serviceCollection.AddAccessTokenManagement((services, clientAccessOptions) =>
        {
            var gigyaOptions = services.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
            clientAccessOptions.Client.Clients.Add($"{name}-idp", new ClientCredentialsTokenRequest
            {
                Address = gigyaOptions.Get(name).OAuthSettings.Token,
                // ClientId = gigyaOptions.Get(name).OAuthSettings.ClientId,
                // ClientSecret = gigyaOptions.Get(name).OAuthSettings.ClientSecret,
                // AuthorizationHeaderStyle = BasicAuthenticationHeaderStyle.Rfc2617,
                Parameters = new Parameters(new KeyValuePair<string, string>[]
                {
                    new("apiKey", gigyaOptions.Get(name).Site.ApiKey),
                    new("userKey", gigyaOptions.Get(name).Creds.UserKey),
                    new("secret", gigyaOptions.Get(name).Creds.Secret),
                }),
                Scope = "api",
                GrantType = "none",


            });

            Console.Out.WriteLine("idp name: " + name);
            Console.Out.WriteLine("token: " + gigyaOptions.Get(name).OAuthSettings.Token);
        });


        serviceCollection.AddSingletonNamedService("ds", (sp, name) =>
        {
            var gigyaOptions = sp.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
            return HttpOptionsBuilder.Configure(builder =>
            {
                var changeTokenSource = builder.ConfigureOptionsBuilder(options =>
                    {
                        // options.Services.AddAccessTokenManagement();
                        // options.Services.AddAccessTokenManagement((services, clientAccessOptions) =>
                        // {
                        //     clientAccessOptions.Client.Clients.Add($"{name}-idp", new ClientCredentialsTokenRequest
                        //     {
                        //         Address = gigyaOptions.Get(name).OAuthSettings.Token,
                        //         ClientId = gigyaOptions.Get(name).OAuthSettings.ClientId,
                        //         ClientSecret = gigyaOptions.Get(name).OAuthSettings.ClientSecret,
                        //         Scope = "api",
                        //         GrantType = "none"
                        //     });
                        //
                        //     Console.Out.WriteLine("idp name: " + name);
                        //     Console.Out.WriteLine("token: " + gigyaOptions.Get(name).OAuthSettings.Token);
                        // });
                        options.Services.AddSingleton((_) =>
                            sp.GetRequiredService<IClientAccessTokenManagementService>());
                        options.Services.AddSingleton((_) => sp.GetRequiredService<IOptionsMonitor<GigyaOptions>>());


                        options.Services.AddHttpClientOptions((serviceName, clientOptions) =>
                        {
                            Console.Out.WriteLine("serviceName: " + serviceName);


                            clientOptions.ServiceName = serviceName;
                            clientOptions.Connection.Server = gigyaOptions.Get(serviceName).Api.accounts.Domain();
                            clientOptions.Connection.Schema = "https";
                            clientOptions.Connection.Port = 443;
                            clientOptions.AddClientAccessTokenHandler($"{name}-idp");
                            Console.Out.WriteLine("name: " + name);
                            Console.Out.WriteLine("domain: " + gigyaOptions.Get(name).Api.accounts.Domain());
                            Console.Out.WriteLine("api: " + gigyaOptions.Get(name).Site.ApiKey);
                        });


                        serviceCollection.AddSingleton((_) => sp.GetRequiredService<IOptionsMonitor<DsOptions>>());

                        // serviceCollection.AddScoped<IConfigureNamedOptions<HttpClientOptions>, GigyaAPIHttpClientConfigurator>();
                    }
                );


                sp.GetRequiredService<IOptionsMonitor<GigyaOptions>>()
                    .OnChange((_) => changeTokenSource.InvokeChange());

                changeTokenSource.GetChangeToken()
                    .RegisterChangeCallback((options) =>
                            (options as IOptionsMonitorCache<DsOptions>)?.Clear(),
                        sp.GetRequiredService<IOptionsMonitorCache<DsOptions>>());
            }).Build();
        });
    }


    public static void AddGigyaApi(this IServiceCollection serviceCollection, Action<GigyaOptions>? configure)
    {
        serviceCollection
            .AddOptions<GigyaOptions>(Options.DefaultName)
            .Configure(configure ?? (_ => { }));

        serviceCollection.AddScoped(provider => provider.GetRequiredService<IOptions<GigyaOptions>>().Value.Api);
    }
}

// public class ProtectedApiBearerTokenHandler : DelegatingHandler
// {
//     public ProtectedApiBearerTokenHandler(
//         IIdentityServerClient identityServerClient)
//     {
//         _identityServerClient = identityServerClient
//                                 ?? throw new ArgumentNullException(nameof(identityServerClient));
//     }
//
//     protected override async Task<HttpResponseMessage> SendAsync(
//         HttpRequestMessage request,
//         CancellationToken cancellationToken)
//     {
//         // request the access token
//         var accessToken = await _identityServerClient.RequestClientCredentialsTokenAsync();
//
//         // set the bearer token to the outgoing request
//         request.Headers.Authorization(accessToken);
//
//         // Proceed calling the inner handler, that will actually send the request
//         // to our protected api
//         return await base.SendAsync(request, cancellationToken);
//     }
// }

public static class DsStorageFactory
{
    public static IDsTypedStore Create(IServiceProvider services, HttpClientCollection httpClientCollection,
        string name)
    {
        var gigyaOptionsMonitor = services.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
        var dsOptions = Options.Create(services.GetRequiredService<IOptionsMonitor<DsOptions>>().Get(name));
        dsOptions.Value.Type ??= dsOptions.Value.Type ?? name;
        return ActivatorUtilities.CreateInstance<DsTypedStore>(services,
            Options.Create(gigyaOptionsMonitor.Get(name)), dsOptions,
            httpClientCollection.CreateClient(name));
    }

    public static IDsTypedStore Create(IServiceProvider services,
        string name)
    {
        var gigyaOptionsMonitor = services.GetRequiredService<IOptionsMonitor<GigyaOptions>>();
        var dsOptions = Options.Create(services.GetRequiredService<IOptionsMonitor<DsOptions>>().Get(name));
        dsOptions.Value.Type ??= dsOptions.Value.Type ?? name;
        return ActivatorUtilities.CreateInstance<DsTypedStore>(services,
            Options.Create(gigyaOptionsMonitor.Get(name)), dsOptions,
            services.GetRequiredServiceByName<HttpClientCollection>("ds").CreateClient(name));
    }
}

public class StoreOptions
{
    public IDsTypedStore Store;
}

public class DsOptions
{
    public string Type;
}

public class DsOptions<T> : DsOptions
{
    public DsOptions()
    {
        Type = nameof(T);
    }
}

public class GigyaOptions
{
    public GigyaSite Site = GigyaSite.Test;
    public GigyaCreds Creds = GigyaCreds.Test;
    public GigyaOidcProvider OAuthSettings => new GigyaOidcProvider(Api, Site, Creds);

    public Api Api => new Api(Site, Creds);
}