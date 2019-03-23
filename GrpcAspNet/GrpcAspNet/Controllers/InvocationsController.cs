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
                _languageWorkerChannel = _functionDispatcher.WorkerChannels.FirstOrDefault();
            }
            
            
            return new string[] { "value1", "value2" };
        }

        // GET: api/Invocations/5
        [HttpGet("{id}", Name = "Get")]
        public Task<string> Get(int id)
        {
            if (_languageWorkerChannel == null)
            {
                _languageWorkerChannel = _functionDispatcher.WorkerChannels.FirstOrDefault();
            }
            ScriptInvocationContext invocationContext = new ScriptInvocationContext()
            {
                FunctionId = id.ToString(),
                InvocationId = Guid.NewGuid().ToString(),
                ResultSource = new TaskCompletionSource<string>()
            };
            _languageWorkerChannel.SendInvocationRequest(invocationContext, id.ToString());
            return invocationContext.ResultSource.Task;
            //return $"{id}-succeeed-{invocationContext.InvocationId}";
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
