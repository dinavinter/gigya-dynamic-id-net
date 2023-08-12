using Stateless.Web;

namespace InteractionApi;

public class InteractionDefinition
{
    private readonly Action<InteractionStateMachine> configuration;

    public InteractionDefinition(
        string name,
        string initialState,
        Action<InteractionStateMachine> configuration = null,
        TimeSpan? ttl = null)
    {
        this.Name = name;
        this.InitialState = initialState;
        this.configuration = configuration;
        this.Ttl = ttl;
    }

    public string Name { get; }

    public string InitialState { get; }

    public TimeSpan? Ttl { get; }

    public InteractionStateMachine CreateInstance(StateMachineContext context, ITransitionDispatcher dispatcher)
    {
        context.Name = this.Name;
        context.State ??= this.InitialState;
        context.Ttl = this.Ttl;
        return InteractionStateMachine.Create(context, dispatcher, this.configuration);
    }
}
