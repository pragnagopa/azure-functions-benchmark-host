using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestGrpc.Messages;

namespace GrpcAspNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvocationsController : ControllerBase
    {
        private LanguageWorkerChannel _languageWorkerChannel;
        private IFunctionDispatcher _functionDispatcher;
        private FunctionRpc.FunctionRpcBase _rpcServerImpl;

        public InvocationsController(IFunctionDispatcher functionDispatcher, FunctionRpc.FunctionRpcBase rpcService)
        {
            _functionDispatcher = functionDispatcher;
            _rpcServerImpl = rpcService;

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
            var invokeId = Guid.NewGuid().ToString();
            InvocationRequest invocationRequest = new InvocationRequest()
            {
                FunctionId = id.ToString(),
                InvocationId = invokeId
            };
            var strMsg = new StreamingMessage
            {
                InvocationRequest = invocationRequest
            };
            ScriptInvocationContext invocationContext = new ScriptInvocationContext()
            {
                FunctionId = id.ToString(),
                InvocationId = invokeId,
                ResultSource = new TaskCompletionSource<string>(),
                Msg = strMsg
            };
            _languageWorkerChannel.SendInvocationRequest(invocationContext);
            FunctionRpcService myservice = _rpcServerImpl as FunctionRpcService;
            myservice.invocationBag.Add(invocationContext);

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
