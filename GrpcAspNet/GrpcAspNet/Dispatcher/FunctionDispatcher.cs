using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public class FunctionDispatcher : IFunctionDispatcher
    {
        public IList<LanguageWorkerChannel> _workerChannels;
        private readonly ILogger _logger;

        public FunctionDispatcher(ILogger<FunctionDispatcher> logger)
        {
            _workerChannels = new List<LanguageWorkerChannel>();
            _logger = logger;
        }
        public IEnumerable<LanguageWorkerChannel> WorkerChannels => _workerChannels;

        public void AddWorkerChannel(LanguageWorkerChannel workerChannel)
        {
            _logger.LogInformation($"Adding LanguageWorkerChannel workerId:{workerChannel.Id}");
            _workerChannels.Add(workerChannel);
        }
    }
}
