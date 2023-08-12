using InteractionApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Stateless.Web;

namespace InteractionApi;

public static class ServiceCollectionExtensions
{
    public static void AddInteraction(this IServiceCollection services)
    {

         services.AddStateless(o =>
        {
            o.UseLiteDBStorage();
            // o.AddStateMachine(
            //     name: nameof(InteractionStateMachine),
            //     initialState: InteractionStateMachine.States.Idle,
            //     InteractionStateMachine.Configure,
            //     ttl: TimeSpan.FromHours(1));
        });
         services.AddScoped<InteractionDefinition>(  sp=> new InteractionDefinition("machine",InteractionStateMachine.States.Idle, InteractionStateMachine.Configure ) );
    }



    public static void UseInteraction(this IApplicationBuilder app )
    {

        app.UseStateless();


    }
}