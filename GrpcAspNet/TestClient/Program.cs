using Google.Protobuf;
using Grpc.Core;
using System;
using System.Threading;
using TestGrpc.Messages;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel(args[0], ChannelCredentials.Insecure);
            var client = new FunctionRpcClient(new FunctionRpc.FunctionRpcClient(channel), args[1]);
            client.RpcStream().GetAwaiter().GetResult();
        }
    }
}
