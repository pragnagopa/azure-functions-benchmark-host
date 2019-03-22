using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public interface IFunctionDispatcher
    {
        IEnumerable<LanguageWorkerChannel> WorkerChannels { get; }

        void AddWorkerChannel(LanguageWorkerChannel workerChannel);
    
    }
}
