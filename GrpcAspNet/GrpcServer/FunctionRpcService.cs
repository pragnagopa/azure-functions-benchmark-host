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

namespace GrpcServer
{
    // Implementation for the grpc service
    // TODO: move to WebJobs.Script.Grpc package and provide event stream abstraction
    internal class FunctionRpcService : FunctionRpc.FunctionRpcBase
    {
        private IScriptEventManager _eventManager;
        private string _workerId;
        private IObservable<InboundEvent> _inboundWorkerEvents;
        private List<IDisposable> _eventSubscriptions = new List<IDisposable>();

        public FunctionRpcService(string workerId)
        {
            _workerId = workerId;
            _eventManager = new ScriptEventManager();
            _inboundWorkerEvents = _eventManager.OfType<InboundEvent>()
               .ObserveOn(new NewThreadScheduler())
               .Where(msg => msg.WorkerId == _workerId);

            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "0")));

            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "1")));

            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "2")));
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "3")));
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "4")));
            Environment.SetEnvironmentVariable("GRPC_EXPERIMENTAL_DISABLE_FLOW_CONTROL", "1");
            Environment.SetEnvironmentVariable("GRPC_CLIENT_CHANNEL_BACKUP_POLL_INTERVAL_MS Default", "0");
        }
        internal void InvocationRequest(StreamingMessage serverMessage, string eventStreamId)
        {
            InvocationRequest invocationRequest = serverMessage.InvocationRequest;
            InvocationResponse invocationResponse = new InvocationResponse()
            {
                InvocationId = invocationRequest.InvocationId,
                Result = "Success"
            };
            StreamingMessage responseMessage = new StreamingMessage()
            {
                InvocationResponse = invocationResponse
            };
            _eventManager.Publish(new OutboundEvent(_workerId, responseMessage, eventStreamId));
        }

        public override async Task EventStream(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();

            var cancelSource = new TaskCompletionSource<bool>();
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
                    if (outboundEventSubscriptions.TryGetValue(_workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "0")
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    responseStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(_workerId, requestStream.Current));
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

        public override async Task EventStream1(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();

            var cancelSource = new TaskCompletionSource<bool>();
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
                    if (outboundEventSubscriptions.TryGetValue(_workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "1")
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    responseStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(_workerId, requestStream.Current));
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

        public override async Task EventStream2(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();

            var cancelSource = new TaskCompletionSource<bool>();
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
                    if (outboundEventSubscriptions.TryGetValue(_workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "2")
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    responseStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(_workerId, requestStream.Current));
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

        public override async Task EventStream3(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();

            var cancelSource = new TaskCompletionSource<bool>();
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
                    if (outboundEventSubscriptions.TryGetValue(_workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "3")
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    responseStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(_workerId, requestStream.Current));
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

        public override async Task EventStream4(IAsyncStreamReader<StreamingMessage> requestStream, IServerStreamWriter<StreamingMessage> responseStream, ServerCallContext context)
        {
            IDictionary<string, IDisposable> outboundEventSubscriptions = new Dictionary<string, IDisposable>();

            var cancelSource = new TaskCompletionSource<bool>();
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
                    if (outboundEventSubscriptions.TryGetValue(_workerId, out outboundEventSubscription))
                    {
                        // no-op
                    }
                    else
                    {
                        outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                            .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "4")
                            .ObserveOn(NewThreadScheduler.Default)
                            .Subscribe(evt =>
                            {
                                try
                                {
                                    // WriteAsync only allows one pending write at a time
                                    // For each responseStream subscription, observe as a blocking write, in series, on a new thread
                                    // Alternatives - could wrap responseStream.WriteAsync with a SemaphoreSlim to control concurrent access
                                    responseStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                }
                                catch (Exception subscribeEventEx)
                                {
                                    // _logger.LogError(subscribeEventEx, $"Error writing message to Rpc channel worker id: {workerId}");
                                }
                            }));
                    }
                    do
                    {
                        _eventManager.Publish(new InboundEvent(_workerId, requestStream.Current));
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
