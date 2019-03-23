using Google.Protobuf;
using Grpc.Core;
using System;
using TestGrpc.Messages;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Environment.SetEnvironmentVariable("GRPC_TRACE", "api");
            //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "debug");
            //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
            Channel channel = new Channel(args[0], ChannelCredentials.Insecure);
            var client = new FunctionRpcClient(new FunctionRpc.FunctionRpcClient(channel), args[1]);
            client.RpcStream();

            while (true) { }
        }
    }
}
