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

namespace GrpcAspNet
{
    public class FunctionRpcClient
    {
        private readonly FunctionRpc.FunctionRpcClient client;
        private readonly IScriptEventManager _eventManager;
        private string _workerId;
        private IObservable<InboundEvent> _inboundWorkerEvents;
        private IObservable<OutboundEvent> _outboundWorkerEvents;
        private List<IDisposable> _eventSubscriptions = new List<IDisposable>();
        IClientStreamWriter<StreamingMessage> _channel4;
        IClientStreamWriter<StreamingMessage> _channel3;
        IClientStreamWriter<StreamingMessage> _channel2;
        IClientStreamWriter<StreamingMessage> _channel1;
        IClientStreamWriter<StreamingMessage> _channel0;

        public FunctionRpcClient(FunctionRpc.FunctionRpcClient client, string workerId, IScriptEventManager scriptEventManager)
        {
            this.client = client;
            _eventManager = scriptEventManager;
            _workerId = workerId;
            _outboundWorkerEvents = _eventManager.OfType<OutboundEvent>()
                    .ObserveOn(new NewThreadScheduler())
                    .Where(msg => msg.WorkerId == _workerId);
        }

        public async void RpcStream()
        {
            IDictionary<string, IDisposable> _eventSubscriptions = new Dictionary<string, IDisposable>();
            _eventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                    .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "0")
                                    .ObserveOn(NewThreadScheduler.Default)
                                    .Subscribe(evt =>
                                    {
                                        _channel0.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                    }));
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

             
                _channel0 = call.RequestStream;
                await responseReaderTask;
            }
        }

        public async void RpcStream1()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                    .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "1")
                                    .ObserveOn(NewThreadScheduler.Default)
                                    .Subscribe(evt =>
                                    {
                                        _channel1.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                    }));
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
                _channel1 = call.RequestStream;
                await responseReaderTask;
            }
        }

        public async void RpcStream2()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                      .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "2")
                                      .ObserveOn(NewThreadScheduler.Default)
                                      .Subscribe(evt =>
                                      {
                                          _channel2.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                      }));
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
                _channel2 = call.RequestStream;
                await responseReaderTask;
            }
        }

        public async void RpcStream3()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                       .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "3")
                                       .ObserveOn(NewThreadScheduler.Default)
                                       .Subscribe(evt =>
                                       {
                                           _channel3.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                       }));
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
                _channel3 = call.RequestStream;
                await responseReaderTask;
            }
        }

        public async void RpcStream4()
        {
            IDictionary<string, IDisposable> _outboundEventSubscriptions = new Dictionary<string, IDisposable>();
            _outboundEventSubscriptions.Add(_workerId, _eventManager.OfType<OutboundEvent>()
                                 .Where(evt => evt.WorkerId == _workerId && evt.EventStreamId == "4")
                                 .ObserveOn(NewThreadScheduler.Default)
                                 .Subscribe(evt =>
                                 {
                                     _channel4.WriteAsync(evt.Message).GetAwaiter().GetResult();
                                 }));
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
                _channel4 = call.RequestStream;
                await responseReaderTask;
            }
        }
    }
}
