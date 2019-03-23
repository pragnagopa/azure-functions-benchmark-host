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
        private List<IDisposable> _eventSubscriptions = new List<IDisposable>();

        public FunctionRpcClient(FunctionRpc.FunctionRpcClient client, string workerId)
        {
            this.client = client;
            _workerId = workerId;
            _eventManager = new ScriptEventManager();
            _inboundWorkerEvents = _eventManager.OfType<InboundEvent>()
                .ObserveOn(new NewThreadScheduler())
                .Where(msg => msg.WorkerId == _workerId);
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

        public async void RpcStream()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "0")));
            using (var call = client.EventStream())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage, "0"));
                    }
                });
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "0")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           call.RequestStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId,
                     EventStreamId = "0"
                };
                StreamingMessage startStream = new StreamingMessage()
                {
                    StartStream = str
                };
                await call.RequestStream.WriteAsync(startStream);
                await responseReaderTask;
            }
        }

        public async void RpcStream1()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "1")));
            using (var call = client.EventStream1())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "1")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           call.RequestStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId,
                     EventStreamId = "1"
                    
                };
                StreamingMessage startStream = new StreamingMessage()
                {
                    StartStream = str
                };
                await call.RequestStream.WriteAsync(startStream);
                await responseReaderTask;
            }
        }

        public async void RpcStream2()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "2")));
            using (var call = client.EventStream2())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "2")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           call.RequestStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId,
                     EventStreamId = "2"
                };
                StreamingMessage startStream = new StreamingMessage()
                {
                    StartStream = str
                };
                await call.RequestStream.WriteAsync(startStream);
                await responseReaderTask;
            }
        }

        public async void RpcStream3()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "3")));
            using (var call = client.EventStream3())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "3")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           call.RequestStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId,
                    EventStreamId = "3"
                };
                StreamingMessage startStream = new StreamingMessage()
                {
                    StartStream = str
                };
                await call.RequestStream.WriteAsync(startStream);
                await responseReaderTask;
            }
        }

        public async void RpcStream4()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_inboundWorkerEvents.Where(msg => msg.MessageType == MsgType.InvocationRequest)
              .ObserveOn(new NewThreadScheduler())
              .Subscribe((msg) => InvocationRequest(msg.Message, "4")));
            using (var call = client.EventStream4())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var serverMessage = call.ResponseStream.Current;
                        _eventManager.Publish(new InboundEvent(_workerId, serverMessage));
                    }
                });
                _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "4")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           call.RequestStream.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
                StartStream str = new StartStream()
                {
                    WorkerId = _workerId,
                    EventStreamId = "4"
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
