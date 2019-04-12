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
using GrpcMessages.Events;
using MsgType = TestGrpc.Messages.StreamingMessage.ContentOneofCase;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GrpcAspNet
{
    // Implementation for the grpc service
    // TODO: move to WebJobs.Script.Grpc package and provide event stream abstraction
    internal class FunctionRpcService : FunctionRpc.FunctionRpcBase
    {
        private IScriptEventManager _eventManager;
        private ILogger _logger;
        IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();
        private static SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);
        public ConcurrentBag<RpcWriteContext> bag = new ConcurrentBag<RpcWriteContext>();
        public ConcurrentBag<ScriptInvocationContext> invocationBag = new ConcurrentBag<ScriptInvocationContext>();

        public FunctionRpcService(IScriptEventManager scriptEventManager, ILogger<FunctionRpcService> logger)
        {
            _eventManager = scriptEventManager;
            _logger = logger;
            //Environment.SetEnvironmentVariable("GRPC_EXPERIMENTAL_DISABLE_FLOW_CONTROL", "1");
            //Environment.SetEnvironmentVariable("GRPC_CLIENT_CHANNEL_BACKUP_POLL_INTERVAL_MS Default", "0");
        }


        public override async Task EventStream(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            var cancelSource = new TaskCompletionSource<bool>();
            try
            {
                context.CancellationToken.Register(() => cancelSource.TrySetResult(false));

                Func<Task<bool>> messageAvailable = async () =>
                {
                    // GRPC does not accept cancellation tokens for individual reads, hence wrapper
                    var requestTask = requestStream.MoveNext(CancellationToken.None);
                    var completed = await Task.WhenAny(cancelSource.Task, requestTask);
                    return completed.Result;
                };

                if (await messageAvailable())
                {
                    Stopwatch stopWatch = new Stopwatch();
                    var requestCount = 0;
                    string workerId = requestStream.Current.StartStream.WorkerId;
                    _logger.LogInformation($"Received start stream..workerId: {workerId}");
                    InboundEvent startStreamEvent = new InboundEvent(workerId, requestStream.Current);
                    startStreamEvent.requestStream = requestStream;
                    _eventManager.Publish(startStreamEvent);
                    do
                    {
                        stopWatch.Start();
                        if (invocationBag.TryTake(out ScriptInvocationContext invocationContext))
                        {
                            await responseStream.WriteAsync(invocationContext.Msg);
                        }
                        if (bag.TryTake(out RpcWriteContext rpcWriteContext))
                        {
                            await responseStream.WriteAsync(rpcWriteContext.Msg);
                            rpcWriteContext.ResultSource.SetResult($"Done writing invocationid:{rpcWriteContext.InvocationId}");
                        }
                        requestCount++;
                    } while (stopWatch.ElapsedMilliseconds < TimeSpan.FromMinutes(10).TotalMilliseconds);
                    //while (requestCount < 10000);
                    _logger.LogInformation($"done...sent 10000");
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

        public override async Task EventStream1(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            var cancelSource = new TaskCompletionSource<bool>();
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            context.CancellationToken.Register(() => cancelSource.TrySetResult(false));
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
                _logger.LogDebug("Established Rpc channel. WorkerId: {workerId}", workerId);
                do
                {
                    var currentMessage = requestStream.Current;
                    if (currentMessage.InvocationResponse != null && !string.IsNullOrEmpty(currentMessage.InvocationResponse.InvocationId))
                    {
                        _logger.LogDebug("Received invocation response for invocationId: {invocationId} from workerId: {workerId}", currentMessage.InvocationResponse.InvocationId, workerId);
                    }
                    _eventManager.Publish(new InboundEvent(workerId, currentMessage));
                }
                while (await messageAvailable());
            }
        }
    }
}
