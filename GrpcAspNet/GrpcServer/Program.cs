namespace GrpcServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FunctionRpcService functionRpcService = new FunctionRpcService(args[0]);
            IRpcServer rpcServer = new GrpcServer(functionRpcService);
            rpcServer.StartAsync().GetAwaiter().GetResult();
            while (true) { }
        }
    }
}
