﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Grpc.Core;
using TestGrpc.Messages;

namespace GrpcMessages.Events
{
    public class InboundEvent : RpcEvent
    {
        public IAsyncStreamReader<StreamingMessage> requestStream;

        public InboundEvent(string workerId, StreamingMessage message) : base(workerId, message, MessageOrigin.Worker)
        {
        }
    }
}
