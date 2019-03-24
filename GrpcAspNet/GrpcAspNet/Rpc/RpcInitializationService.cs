// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrpcServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GrpcMessages.Events;
using Grpc.Core;
using TestGrpc.Messages;

namespace GrpcAspNet
{
    public class RpcInitializationService : IHostedService
    {
        private readonly IScriptEventManager _eventManager;
        private readonly IFunctionDispatcher _functionDispatcher;
        private LanguageWorkerChannel _languageWorkerChannel;
        private const string serviceUri = "127.0.0.1:50052";
        private string _workerId = Guid.NewGuid().ToString();

        public RpcInitializationService(IFunctionDispatcher functionDispatcher, IScriptEventManager eventManager)
        {
            _eventManager = eventManager;
            _functionDispatcher = functionDispatcher;
        }

        public LanguageWorkerChannel WorkerChannel => _languageWorkerChannel;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeChannelsAsync();
            await InitializeRpcClientAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        internal async Task InitializeRpcClientAsync()
        {
            try
            {
                Environment.SetEnvironmentVariable("GRPC_EXPERIMENTAL_DISABLE_FLOW_CONTROL", "1");
                //GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
                Channel channel = new Channel(serviceUri, ChannelCredentials.Insecure);
                var client = new FunctionRpcClient(new FunctionRpc.FunctionRpcClient(channel), _workerId, _eventManager);
                client.RpcStream();
                client.RpcStream1();
                client.RpcStream2();
                client.RpcStream3();
                client.RpcStream4();
            }
            catch (Exception grpcInitEx)
            {
                var hostInitEx = new Exception($"Failed to start Rpc Server. Check if your app is hitting connection limits.", grpcInitEx);
            }
        }

        internal Task InitializeChannelsAsync()
        {
            _languageWorkerChannel = new LanguageWorkerChannel(_workerId, _eventManager);
            _functionDispatcher.AddWorkerChannel(_languageWorkerChannel);
            //TODO wait for server to start
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }
    }
}
