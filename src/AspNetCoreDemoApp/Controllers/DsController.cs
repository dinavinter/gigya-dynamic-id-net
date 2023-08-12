using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations.Rules;
using Orleans;
using Orleans.Runtime;

namespace AspNetCoreDemoApp.Controllers
{
    [Route("api/[controller]/{type}")]
    [ApiController]
    public class DsController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;


        public DsController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // GET: api/values
        [HttpGet()]

        public async Task<IEntity?> Get(string type, string id)
        {
            var store = _serviceProvider.GetDsStore(type);

            return await store.GetAsync(id);
        }

        // GET api/values/5
        [HttpPut()]
        public async Task<IActionResult> Add(string type, [FromBody] object data)
        {
            var store = _serviceProvider.GetDsStore(type);
            var id = Guid.NewGuid().ToString();
            await store.SaveAsync(id, data);
            return Created($"/ds/{type}/{id}", data);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(string type, string id, [FromBody] object data)
        {
            var store = _serviceProvider.GetDsStore(type);
            await store.SaveAsync(id, data);
            return Accepted();
        }
    }
}