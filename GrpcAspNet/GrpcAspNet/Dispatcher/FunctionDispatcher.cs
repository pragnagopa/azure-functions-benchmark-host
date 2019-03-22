using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public class FunctionDispatcher : IFunctionDispatcher
    {
        public IList<LanguageWorkerChannel> _workerChannels;
        public FunctionDispatcher()
        {
            _workerChannels = new List<LanguageWorkerChannel>();
        }
        public IEnumerable<LanguageWorkerChannel> WorkerChannels => _workerChannels;

        public void AddWorkerChannel(LanguageWorkerChannel workerChannel)
        {
            _workerChannels.Add(workerChannel);
        }
    }
}
