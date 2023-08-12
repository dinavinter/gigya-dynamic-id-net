using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.Json;
using Json.Schema;
using Stateless;
using Stateless.Web;

namespace InteractionApi;

public class EmbeddedResource
{
    public EmbeddedResource()
    {
    }

    public async Task Load(string id = "default")
    {
        var xState = XState.FromJson(await GetEmbeddedResource(id).ConfigureAwait(false));
        var stateMachine = new StateMachine<string, string>(xState.Initial);
        foreach (var state in xState.States)
        {
            var stateBuilder = stateMachine.Configure(state.Key);

            foreach (var subState in state.Value.On)
            {
                foreach (var transition in subState.Value)
                {
                    transition.Target.Aggregate(stateBuilder,
                        (builder, target) => builder.Permit(subState.Key, target));
                }
            }
        }
    }

    // private State GetState(BaseStateNode state)
    // {
    //     var state = new State(state.Id);
    // }

    public static async Task<JsonSchema?> GetEmbeddedSchema(string? id)
    {
        if (id == null)
        {
            return null;
        }
        return await GetEmbeddedResourceStream($"{id}.schema",
            async streamReader => await JsonSchema.FromStream(streamReader.BaseStream)).ConfigureAwait(false);
    }

    public static async Task<JsonDocument?> GetEmbeddedResource(string id)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream($"EmbeddedResource.Data.{id}.json");
        if (resourceStream == null) return null;

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await JsonDocument.ParseAsync(reader.BaseStream);
    }

    public static async Task<T?> GetEmbeddedResourceStream<T>(string id, Func<StreamReader, Task<T>> read)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream($"EmbeddedResource.Data.{id}.json");
        if (resourceStream == null) return default(T);

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await read(reader).ConfigureAwait(false);
    }
}