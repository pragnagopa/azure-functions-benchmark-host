using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public class RpcWriteContext
    {
        public string InvocationId { get; set; }

        public TaskCompletionSource<string> ResultSource { get; set; }
    }
}
