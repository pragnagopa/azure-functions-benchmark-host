using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestGrpc.Messages;
using GrpcMessages.Events;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using MsgType = TestGrpc.Messages.StreamingMessage.ContentOneofCase;
using System.Threading;
using System.Collections.Concurrent;

namespace TestClient
{
    public class FunctionRpcClient
    {
        private readonly FunctionRpc.FunctionRpcClient client;
        private readonly IScriptEventManager _eventManager;
        private string _workerId;
        private IObservable<InboundEvent> _inboundWorkerEvents;
        IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
        private List<IDisposable> _eventSubscriptions = new List<IDisposable>();
        private static SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);
        private ConcurrentBag<StreamingMessage> invokeRes = new ConcurrentBag<StreamingMessage>();

        public FunctionRpcClient(FunctionRpc.FunctionRpcClient client, string workerId)
        {
            this.client = client;
            _workerId = workerId;
            _eventManager = new ScriptEventManager();
            _inboundWorkerEvents = _eventManager.OfType<InboundEvent>()
                .ObserveOn(new NewThreadScheduler())
                .Where(msg => msg.WorkerId == _workerId);

            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
                .ObserveOn(new NewThreadScheduler())
                .Subscribe((msg) => InvocationRequest(msg.Message)));
        }

        internal void InvocationRequest(StreamingMessage serverMessage)
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
            invokeRes.Add(responseMessage);
            //_eventManager.Publish(new OutboundEvent(_workerId, responseMessage));
        }

        public async Task<bool> RpcStream()
        {
            using (var call = client.EventStream())
            {
                var cancelSource = new TaskCompletionSource<bool>();
                Func<Task<bool>> messageAvailable = async () =>
                {
                    // GRPC does not accept cancellation tokens for individual reads, hence wrapper
                    var requestTask = call.ResponseStream.MoveNext(CancellationToken.None);
                    var completed = await Task.WhenAny(cancelSource.Task, requestTask);
                    return completed.Result;
                };

                if (await messageAvailable())
                {
                    do
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                        StreamingMessage res;
                        if (invokeRes.TryTake(out res))
                        {
                            await call.RequestStream.WriteAsync(res);
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    }
                    while (true);
                }


                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId
                };
                StreamingMessage startStream = new StreamingMessage()
                {
                    StartStream = str
                };
                await call.RequestStream.WriteAsync(startStream);
                await responseReaderTask;
                return true;
            }
        }
    }
}
