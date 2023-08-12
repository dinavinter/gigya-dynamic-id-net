using System.Runtime.CompilerServices;
using Json.Schema;
using Stateless;
using Stateless.Web;

namespace InteractionApi;

public enum State
{
}

public class InteractionStateMachine
{
    public struct States
    {
        public const string Idle = "idle";
        public const string Init = "init";
        public const string Submit = "submit";
        public const string Success = "success";
        public const string Error = "error";
        public const string Cancel = "canceld";
    }

    public struct Triggers
    {
        public const string Load = "load";

        public static readonly InteractionEvent Submit = new InteractionEvent()
        {
            Type = "Submit",
            Meta =
            {
                JsonSchema = new JsonSchemaBuilder()
                    .Schema(MetaSchemas.Draft202012Id)
                    .Id("https://json-everything.net/schemas/schema")
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("name", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                        ),
                        ("email", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                        )
                    )
            }
        };


        public const string Cancel = "Cancel";
        public const string Success = "success";
        public const string Error = "error";
        public const string Retry = "retry";
    }

    public readonly StateMachine<string, InteractionEvent> Machine;

    private InteractionStateMachine(string state) => this.Machine = new StateMachine<string, InteractionEvent>(state);

    public IEnumerable<InteractionEvent> PermittedTriggers => this.Machine.PermittedTriggers;

    public StateMachineContext Context { get; private set; }

    public ITransitionDispatcher Dispatcher { get; private set; }

    //
    public static InteractionStateMachine Create(StateMachineContext context,
        ITransitionDispatcher dispatcher, Action<InteractionStateMachine> configuration)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        InteractionStateMachine stateMachine = new InteractionStateMachine(context.State)
        {
            Context = context,
            Dispatcher = dispatcher
        };

        configuration?.Invoke(stateMachine);
        return stateMachine;
    }


    public static void Configure(InteractionStateMachine stateMachine)
    {
        stateMachine.Configure(States.Idle)
            .OnActivate(() => Console.WriteLine("-Idle!"))
            .OnEntry((t) => Console.WriteLine($"idle interaction {t.Destination}"))
            .Permit(Triggers.Load, States.Init);

        stateMachine.Configure(States.Init)
            .OnActivate(() => Console.WriteLine("-Init!"))
            .OnEntry((t) => Console.WriteLine($"init interaction {t.Destination}"))
            .Permit(Triggers.Submit, States.Submit)
            .Permit(Triggers.Cancel, States.Cancel);

        stateMachine.Configure(States.Submit)
            .OnActivate(() => Console.WriteLine("-Submit!"))
            .OnEntry((t) => Console.WriteLine($"the interaction is submitting {t.Destination}"))
            .Permit(Triggers.Success, States.Success)
            .Permit(Triggers.Error, States.Error);

        stateMachine.Configure(States.Error)
            .OnActivate(() => Console.WriteLine("-Error!"))
            .OnEntry((t) => Console.WriteLine($"the interaction is broken {t.Destination}"))
            .Permit(Triggers.Retry, States.Submit);

        stateMachine.Configure(States.Success)
            .OnActivate(() => Console.WriteLine("-Success!"))
            .OnEntry((t) => Console.WriteLine($"the interaction is broken {t.Destination}"));
    }

    private static async Task EntrySubmitAction(StateMachine<string, string>.Transition transition)
    {
    }

    public void Activate() => this.Machine.Activate();

    public void Deactivate() => this.Machine.Deactivate();

    public async Task<bool> FireAsync(InteractionEvent trigger)
    {
        if (this.Context.IsExpired())
            throw new Exception("statemachine: cannot fire trigger on expired state machines");
        if (!this.Machine.CanFire(trigger))
            return false;
        ConfiguredTaskAwaitable configuredTaskAwaitable = this.Machine.DeactivateAsync().ConfigureAwait(false);
        await configuredTaskAwaitable;
        configuredTaskAwaitable = this.Machine.FireAsync(trigger).ConfigureAwait(false);
        await configuredTaskAwaitable;
        configuredTaskAwaitable = this.Machine.ActivateAsync().ConfigureAwait(false);
        await configuredTaskAwaitable;
        this.Context.State = this.Machine.State;
        return true;
    }

    internal StateMachine<string, InteractionEvent>.StateConfiguration Configure(string state) =>
        this.Machine.Configure(state);
}

public class InteractionEvent : IEquatable<InteractionEvent>
{
    public static implicit operator string?(InteractionEvent @event) => @event.ToString();
    public static implicit operator InteractionEvent(string type) => ToInteractionEvent(type);

    public string Type = "";
    public InteractionMetadata Meta = new InteractionMetadata();


    public override string ToString() => Type;

    public static InteractionEvent ToInteractionEvent(string type) => new() {Type = type};

    public bool Equals(InteractionEvent? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InteractionEvent) obj);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(Type, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(InteractionEvent? left, InteractionEvent? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(InteractionEvent? left, InteractionEvent? right)
    {
        return !Equals(left, right);
    }

    private sealed class TypeEqualityComparer : IEqualityComparer<InteractionEvent>
    {
        public bool Equals(InteractionEvent x, InteractionEvent y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Type == y.Type;
        }

        public int GetHashCode(InteractionEvent obj)
        {
            return obj.Type.GetHashCode();
        }
    }

    public static IEqualityComparer<InteractionEvent> TypeComparer { get; } = new TypeEqualityComparer();
}

public class InteractionMetadata
{
    public string? Schema;
    public JsonSchema? JsonSchema;
    public string? AuthorizationPolicy;

    public Task<JsonSchema?> GetSchema() => EmbeddedResource.GetEmbeddedSchema(Schema);
}