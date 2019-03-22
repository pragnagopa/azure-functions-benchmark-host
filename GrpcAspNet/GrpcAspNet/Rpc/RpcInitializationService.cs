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

namespace GrpcAspNet
{
    public class RpcInitializationService : IHostedService
    {
        private readonly IRpcServer _rpcServer;
        private readonly IScriptEventManager _eventManager;
        private readonly IFunctionDispatcher _functionDispatcher;
        private LanguageWorkerChannel _languageWorkerChannel;

        public RpcInitializationService(IRpcServer rpcServer, IFunctionDispatcher functionDispatcher, IScriptEventManager eventManager)
        {
            _rpcServer = rpcServer;
            _eventManager = eventManager;
            _functionDispatcher = functionDispatcher;
        }

        public LanguageWorkerChannel WorkerChannel => _languageWorkerChannel;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeRpcServerAsync();
            await InitializeChannelsAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _rpcServer.KillAsync();
        }

        internal async Task InitializeRpcServerAsync()
        {
            try
            {
                await _rpcServer.StartAsync();
            }
            catch (Exception grpcInitEx)
            {
                var hostInitEx = new Exception($"Failed to start Rpc Server. Check if your app is hitting connection limits.", grpcInitEx);
            }
        }

        internal Task InitializeChannelsAsync()
        {
            string workerId = Guid.NewGuid().ToString();
            _languageWorkerChannel = new LanguageWorkerChannel(workerId, _eventManager, _rpcServer.CSharpUri);
            _functionDispatcher.AddWorkerChannel(_languageWorkerChannel);
            return Task.CompletedTask;
        }
    }
}
