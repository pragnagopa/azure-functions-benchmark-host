using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public class FunctionDispatcher : IFunctionDispatcher
    {
        public IList<LanguageWorkerChannel> _workerChannels;
        private int _counter = 1;
        private object _functionLoadResponseLock = new object();

        public FunctionDispatcher()
        {
            _workerChannels = new List<LanguageWorkerChannel>();
        }
        public IEnumerable<LanguageWorkerChannel> WorkerChannels => _workerChannels;

        public void AddWorkerChannel(LanguageWorkerChannel workerChannel)
        {
            _workerChannels.Add(workerChannel);
        }

        public int GetEventStreamId()
        {
            var currentNumberOfWorkers = 2;
            var result = _counter % currentNumberOfWorkers;
            lock (_functionLoadResponseLock)
            {
                _counter = _counter == 0 ? 1 : 0;
            }
            return result;
        }
    }
}
