// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GrpcAspNet
{
    public class ScriptInvocationContext
    {
        public string InvocationId { get; set; }

        public string FunctionId { get; set; }

        public TaskCompletionSource<string> ResultSource { get; set; }
    }
}
