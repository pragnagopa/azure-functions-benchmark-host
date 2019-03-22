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
            Environment.SetEnvironmentVariable("GRPC_TRACE", "api");
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "debug");
            Grpc.Core.GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
            Console.WriteLine("Hello World!");
            Channel channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            //var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            var client = new FunctionRpcClient(new FunctionRpc.FunctionRpcClient(channel), args[1]);
            client.RpcStream();

            Console.ReadKey();
        }
    }
}
