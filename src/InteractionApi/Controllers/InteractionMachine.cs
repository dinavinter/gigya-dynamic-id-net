using System.Reflection;
using FluentSiren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Stateless.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;

namespace InteractionApi.Controllers;

[Route("interaction/{name}")]
public class InteractionMachine : Controller
{
    private readonly IStateMachineContextStorage _contextStorage;
    private readonly IStateMachineContentStorage _contentStorage;
    private readonly IEnumerable<InteractionDefinition> _definitions;
    private readonly ITransitionDispatcher _transitionDispatcher;

    public InteractionMachine(IStateMachineContextStorage contextStorage,
        IStateMachineContentStorage contentStorage,
        IEnumerable<InteractionDefinition> definitions,
        ITransitionDispatcher transitionDispatcher)
    {
        _contextStorage = contextStorage;
        _contentStorage = contentStorage;
        _definitions = definitions;
        _transitionDispatcher = transitionDispatcher;
    }

    [HttpPut()]
    [SwaggerOperation(Summary = "Initiate a new instance of interaction", OperationId = "Interaction.Create")]
    [SwaggerOperationFilter(typeof(SwaggerOperationFilter))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerLink(response: "201", key: "getInteraction", operation: "Interaction.Get", "$request.path.name",
        "$response.body#/id")]
    [SwaggerLink(response: "201", key: "triggerInteraction", operation: "Interaction.Trigger", "$request.path.name",
        "$response.body#/id")]
    public async Task<ActionResult<Entity>> Create(string name, [FromBody] CreateMachineRequest createMachineRequest)
    {
        var definition = _definitions.First(definition => definition.Name == name);

        var context = new StateMachineContext() {Created = DateTime.UtcNow, Updated = DateTime.UtcNow};
        foreach (var kvp in createMachineRequest.Context)
        {
            context.Properties.AddOrUpdate(kvp.Key, kvp.Value);
        }

        _contextStorage.Save(context);

        var instance = definition.CreateInstance(context, _transitionDispatcher);

        return CreatedAtAction(nameof(Get), nameof(InteractionMachine), new
        {
            name,
            context.Id
        }, InteractionResponse(instance));

        // return InteractionResponse(name, context.Id, instance);
    }


    [HttpGet("{id}")]
    [SwaggerOperation(OperationId = "Interaction.Get")]
    [SwaggerResponse(200, Type = typeof(Entity), ContentTypes = new[] {"application/vnd.siren+json"})]
    public async Task<ActionResult<Entity>> Get(string name, string id)
    {
        var definition = _definitions.First(definition => definition.Name == name);

        var context = _contextStorage.FindById(id);

        var instance = definition.CreateInstance(context, _transitionDispatcher);


        return Ok(InteractionResponse(instance));
    }

    [HttpPost("{id}/events/{trigger}")]
    [SwaggerOperation(OperationId = "Interaction.Trigger")]
    public async Task<ActionResult<Entity>> Trigger(string name, string id, string trigger)
    {
        var definition = _definitions.First(definition => definition.Name == name);

        var context = _contextStorage.FindById(id);

        var instance = definition.CreateInstance(context, _transitionDispatcher);
        await instance.FireAsync(trigger).ConfigureAwait(false);

        return InteractionResponse(instance);
    }

    private static Entity InteractionResponse(InteractionStateMachine instance)
    {
        return
            new InteractionResponse(instance).SirenEntity;
    }
}

internal class SwaggerSchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo != null)
        {
            var schemaAttribute = context.MemberInfo.GetCustomAttributes<SwaggerSchemaExampleAttribute>()
                .FirstOrDefault();
            if (schemaAttribute != null)
                ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private void ApplySchemaAttribute(OpenApiSchema schema, SwaggerSchemaExampleAttribute schemaAttribute)
    {
        if (schemaAttribute.Example != null)
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(schemaAttribute.Example);
        }
    }
}

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Enum,
    AllowMultiple = false)]
public class SwaggerSchemaExampleAttribute : Attribute
{
    public SwaggerSchemaExampleAttribute(string example)
    {
        Example = example;
    }

    public string Example { get; set; }
}

[AttributeUsage(
    AttributeTargets.Method,
    AllowMultiple = true)]
public class SwaggerLinkAttribute : Attribute
{
    public SwaggerLinkAttribute(string response, string key, string operation, params string[] parameters)
    {
        Response = response;
        Key = key;
        Operation = operation;
        Parameters = parameters;
    }

    public string Response { get; private set; }
    public string Key { get; private set; }
    public string Operation { get; }
    public string[] Parameters { get; }

    public string Link { get; private set; }
}

internal class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var linksAttributes = context.MethodInfo.GetCustomAttributes<SwaggerLinkAttribute>(true);

        foreach (var attribute in linksAttributes)
        {
            ApplyLinkAttribute(operation.Responses[attribute.Response], attribute);
        }
    }


    private void ApplyLinkAttribute(OpenApiResponse response, SwaggerLinkAttribute linksAttribute)
    {
        var link = new OpenApiLink();
        foreach (var parameter in linksAttribute.Parameters)
        {
            link.Parameters[parameter.Split(".").Last().Split('/').Last()] = new RuntimeExpressionAnyWrapper
            {
                Any = new OpenApiString(parameter)
            };
        }

        link.OperationId = linksAttribute.Operation;

        response.Links.Add(new KeyValuePair<string, OpenApiLink>(linksAttribute.Key, link));
    }
}

public class TriggerRequest
{
}

public class CreateMachineRequest
{
    public Dictionary<string, object> Context = new Dictionary<string, object>();
}