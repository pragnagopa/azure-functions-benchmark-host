// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using TestGrpc.Messages;

namespace GrpcMessages.Events
{
    public class InboundEvent : RpcEvent
    {
        public InboundEvent(string workerId, StreamingMessage message, string eventStreamId = "0") : base(workerId, message, eventStreamId, MessageOrigin.Worker)
        {
        }
    }
}
