using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestGrpc.Messages;

namespace TestClient
{
    public class FunctionRpcClient
    {
        readonly FunctionRpc.FunctionRpcClient client;
        string _workerId;
        IClientStreamWriter<StreamingMessage> _requestStream;

        public FunctionRpcClient(FunctionRpc.FunctionRpcClient client, string workerId)
        {
            this.client = client;
            _workerId = workerId;
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
                        await EventStreamWriteAsync(responseMessage);
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
                _requestStream = call.RequestStream;
                // await EventStreamWriteAsync(startStream);
                await responseReaderTask;
            }
        }

        public async Task EventStreamWriteAsync(StreamingMessage message)
        {
            await _requestStream.WriteAsync(message);
        }
    }
}
