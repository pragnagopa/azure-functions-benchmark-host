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
            Environment.SetEnvironmentVariable("GRPC_EXPERIMENTAL_DISABLE_FLOW_CONTROL", "1");
            //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
            Channel channel = new Channel(args[0], ChannelCredentials.Insecure);
            var client = new FunctionRpcClient(new FunctionRpc.FunctionRpcClient(channel), args[1]);
            client.RpcStream();
            client.RpcStream1();
            client.RpcStream2();
            client.RpcStream3();
            client.RpcStream4();

            while (true) { }
        }
    }
}
