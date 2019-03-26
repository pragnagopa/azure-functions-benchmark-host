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
            _eventManager.Publish(new OutboundEvent(_workerId, responseMessage));
        }

        public async void RpcStream()
        {
            using (var call = client.EventStream())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                EventLoopScheduler eventLoopScheduler = new EventLoopScheduler();
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId)
                                       .ObserveOn(eventLoopScheduler)
                                       .Subscribe(async evt =>
                                       {
                                           await call.RequestStream.WriteAsync(evt.Message);
                                       }));
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
            }
        }
    }
}
