using System;
using System.Linq;
using System.Threading.Tasks;
using DSStore;
using Microsoft.AspNetCore.Mvc;
using TryJsonEverything.Models;
using TryJsonEverything.Services;

namespace AspNetCoreDemoApp.Controllers;

[Route("interaction/{type}/schema")]
public class SchemaController : Controller
{
    private readonly IDsTypedStore _store;

    public SchemaController(IServiceProvider serviceProvider)
    {
        _store = serviceProvider.GetDsStore("interaction-metadata");
    }

    [HttpGet()]
    public async Task<ActionResult<InteractionSchema?>> Get(string type)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

        var schema = await _store.GetAsync(type);

        return Ok(schema?.Get<InteractionSchema>() ?? new InteractionSchema());
    }

    [HttpPost()]
    public ActionResult<SchemaValidationOutput> Set(string type, [FromBody] InteractionSchema input)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

        _store.SaveAsync(type, input);

        return CreatedAtRoute($"/interaction/{type}/schema", input);
    }
}