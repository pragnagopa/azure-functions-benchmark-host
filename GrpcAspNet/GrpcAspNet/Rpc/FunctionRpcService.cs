// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using TestGrpc.Messages;
using MsgType = TestGrpc.Messages.StreamingMessage.ContentOneofCase;

namespace GrpcAspNet
{
    // Implementation for the grpc service
    // TODO: move to WebJobs.Script.Grpc package and provide event stream abstraction
    internal class FunctionRpcService : FunctionRpc.FunctionRpcBase
    {
        private IAsyncStreamReader<StreamingMessage> _requestStream;
        private IServerStreamWriter<StreamingMessage> _responseStream;
        private IScriptEventManager _eventManager;
        public FunctionRpcService(IScriptEventManager scriptEventManager)
        {
            _eventManager = scriptEventManager;
        }

        public override async Task EventStream(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            var cancelSource = new TaskCompletionSource<bool>();

            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            try
            {
                context.CancellationToken.Register(() => cancelSource.TrySetResult(false));
                IDisposable outboundEventSubscription = null;

                Func<Task<bool>> messageAvailable = async () =>
                {
                    // GRPC does not accept cancellation tokens for individual reads, hence wrapper
                    var requestTask = requestStream.MoveNext(CancellationToken.None);
                    var completed = await Task.WhenAny(cancelSource.Task, requestTask);
                    return completed.Result;
                };

                if (await messageAvailable())
                {
                    string workerId = requestStream.Current.StartStream.WorkerId;
                    if (outboundEventSubscriptions.TryGetValue(workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == workerId)
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(async evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    await responseStream.WriteAsync(evt.Message);
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(workerId, requestStream.Current));
                    }
                    while (await messageAvailable());
                }
            }
            finally
            {
                foreach (var sub in outboundEventSubscriptions)
                {
                    sub.Value?.Dispose();
                }

                // ensure cancellationSource task completes
                cancelSource.TrySetResult(false);
            }
        }
    }
}
