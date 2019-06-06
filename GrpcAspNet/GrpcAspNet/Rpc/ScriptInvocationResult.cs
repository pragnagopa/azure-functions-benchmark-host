using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAspNet
{
    public class ScriptInvocationResult
    {
        public object Return { get; set; }

        public IDictionary<string, object> Outputs { get; set; }
    }
}
