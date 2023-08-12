using System.Collections.Immutable;
using System.Text.Json;
using FluentSiren.Builders;
using FluentSiren.Models;
using FluentSiren.Templates;
using Json.Schema;
using Stateless.Reflection;
using Action = System.Action;

namespace InteractionApi.Controllers;

public class InteractionResponse
{
    private readonly InteractionStateMachine _instance;

    public InteractionResponse(InteractionStateMachine instance)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public Entity ToSirenEntity()
    {
        var entity = _instance.PermittedTriggers
                ?
            .Aggregate(new EntityBuilder()
                    .WithClass(_instance.Context?.Name)
                    .WithProperty("state", _instance.Context?.State)
                    // .WithSubEntity(_instance.ToMachineEntity())


                    , (builder, trigger) =>
                {

                    var action = new ActionBuilder()
                        .WithName($"event-{trigger}")
                        .WithTitle(trigger)
                        .WithMethod("POST")
                        .WithHref($"/{_instance.Context!.Name}/{_instance.Context.Id}/events/{trigger}")
                        .WithType("application/json")
                        .BuildAction(trigger.Meta.JsonSchema ?? JsonSchema.Empty);


                    // action = trigger.Meta?.JsonSchema?.Keywords?
                    //     .OfType<PropertiesKeyword>()
                    //     .FirstOrDefault()?
                    //     .Properties
                    //     .Select((property) => new FieldBuilder()
                    //         .WithName(property.Key)
                    //         .WithType(property.Value.Keywords?.OfType<TypeKeyword>().FirstOrDefault()?.Type.ToString())
                    //         .WithValue(property.Value.Keywords?.OfType<DefaultKeyword>().FirstOrDefault()?.Value
                    //             .ToString()))
                    //     .Aggregate(action, (actionBuilder, fieldBuilder) => actionBuilder.WithField(fieldBuilder));
                    //
                    return builder.WithAction(action);
                }

                // .WithField(new FieldBuilder()
                //     .WithName("orderNumber")
                //     .WithType("hidden")
                //     .WithValue("42"))
                // .WithField(new FieldBuilder()
                //     .WithName("productCode")
                //     .WithType("text"))
            ).Build();

        return entity;
    }

    public Entity SirenEntity => ToSirenEntity();


    public string State { get; set; }
    public Dictionary<string, string> Links { get; set; }
    public string Id { get; set; }
}

internal static class PersonExtension
{
    internal static ActionBuilder ToAction(this InteractionEvent eEvent)
    {
       return new ActionBuilder()
            .WithName($"event-{eEvent.Type}")
            .WithTitle(eEvent.Type)
            .WithMethod("POST")
            .WithHref($"/events/{eEvent.Type}")
            .WithType("application/json")
            .BuildAction(eEvent.Meta.JsonSchema ?? JsonSchema.Empty);

    }
    internal static EmbeddedRepresentationBuilder ToMachineEntity(this InteractionStateMachine  machine)
    {
        var machineInfo = machine.Machine.GetInfo();
        var currentState = machineInfo.States
            .FirstOrDefault(x => x.UnderlyingState == machine.Context.State);


        var xstate = new XState()
        {
            Id= machine.Context.State,
            Initial = machine.Context.State,
            Context = machine.Context.Properties,
            States = ToStates(currentState.FixedTransitions.Select(t=>t.DestinationState)),
            On =ToXstateTransitions( currentState)
        };
        static BaseStateNode ToStateNode (StateInfo? state)
        {
            return new BaseStateNode()
            {
                Id= state?.ToString(),
                Key = state?.ToString(),
                On = ToXstateTransitions( state),

            };
        }

          Dictionary<string, BaseStateNode> ToStates(IEnumerable<StateInfo> stateInfo)
        {
            return stateInfo.Select(ToStateNode).ToDictionary(x => x.Id);
        }
        static  Dictionary<string, TransitionObject[]>? ToXstateTransitions(StateInfo? state)
        {
            return state?.FixedTransitions
                .GroupBy(e => e.Trigger.ToString(), info => new TransitionObject()
                {
                    Target = new[] {info.DestinationState.ToString()}
                })
                .ToDictionary(e => e.Key, e => e.ToArray());

        }

        return new EmbeddedRepresentationBuilder()
            .WithClass($"xstate")
            .WithProperty("machine", xstate  )
            .WithRel($"/{machine.Context!.Name}/{machine.Context.Id}");

    }


    // internal static EmbeddedRepresentationBuilder ToRepresentation(this JsonSchema schema)
    // {
    //     return (EmbeddedRepresentationBuilder) Build<EmbeddedRepresentationBuilder>(schema);
    // }

    internal static ActionBuilder BuildAction (this ActionBuilder actionBuilder, JsonSchema schema)
    {

        return BuildAction<ActionBuilder, FluentSiren.Models.Action>(actionBuilder, schema);
    }
    internal static TBuilder BuildAction<TBuilder, TAction  >(this TBuilder actionBuilder, JsonSchema schema)
        where TBuilder :  ActionBuilder<TBuilder,  TAction> where TAction : FluentSiren.Models.Action
    {
         var properties = schema.Keywords?.OfType<PropertiesKeyword>().FirstOrDefault()?.Properties?? ImmutableDictionary<string, JsonSchema>.Empty;
        foreach (var (key, value) in properties)
        {
            actionBuilder = actionBuilder.WithField(BuildField<FieldBuilder, Field>(key, value));
        }

        return actionBuilder;
    }
    private static TBuilder BuildField<TBuilder, TField  >(string name, JsonSchema schema)
        where TBuilder :  FieldBuilder<TBuilder,  TField> where TField : Field
    {
        var type = schema.Keywords?.OfType<TypeKeyword>().FirstOrDefault()?.Type.ToString();
        var title =  schema.Keywords?.OfType<TitleKeyword>().FirstOrDefault()?.Value.ToString();

        return Activator.CreateInstance<TBuilder>()
            .WithClass("field")
            .WithType(type)
            .WithName(name)
            .WithTitle(title);
    }
 }