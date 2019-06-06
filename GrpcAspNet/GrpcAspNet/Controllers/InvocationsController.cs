using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrpcAspNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvocationsController : ControllerBase
    {
        private LanguageWorkerChannel _languageWorkerChannel;
        private IFunctionDispatcher _functionDispatcher;

        public InvocationsController(IFunctionDispatcher functionDispatcher)
        {
            _functionDispatcher = functionDispatcher;

        }

        // GET: api/Invocations
        [HttpGet]
        public IEnumerable<string> Get()
        {
            if (_languageWorkerChannel == null)
            {
                _languageWorkerChannel = _functionDispatcher.WorkerChannel;
            }


            return new string[] { "value1", "value2" };
        }

        // GET: api/Invocations/5
        [HttpGet("{id}")]
        public Task<string> Get(int id)
        {
            if (_languageWorkerChannel == null)
            {
                _languageWorkerChannel = _functionDispatcher.WorkerChannel;
            }
            ScriptInvocationContext scriptInvocationContext = new ScriptInvocationContext()
            {
                FunctionId = id.ToString(),
                InvocationId = Guid.NewGuid().ToString(),
                ResultSource = new TaskCompletionSource<string>()
            };
            _languageWorkerChannel.SendInvocationRequest(scriptInvocationContext);
            return scriptInvocationContext.ResultSource.Task;
        }

        // POST: api/Invocations
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Invocations/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
