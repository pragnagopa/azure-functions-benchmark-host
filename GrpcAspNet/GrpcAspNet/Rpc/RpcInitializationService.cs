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

namespace GrpcAspNet
{
    public class RpcInitializationService : IHostedService
    {
        private readonly IRpcServer _rpcServer;
        private readonly IScriptEventManager _eventManager;
        private readonly IFunctionDispatcher _functionDispatcher;
        private LanguageWorkerChannel _languageWorkerChannel;
        private ILogger _logger;

        public RpcInitializationService(IRpcServer rpcServer, IFunctionDispatcher functionDispatcher, IScriptEventManager eventManager, ILogger<RpcInitializationService> logger)
        {
            _rpcServer = rpcServer;
            _eventManager = eventManager;
            _functionDispatcher = functionDispatcher;
            _logger = logger;
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
                _logger.LogInformation("Starting grpc server");
                await _rpcServer.StartAsync();
                _logger.LogInformation("Done Starting grpc server");
            }
            catch (Exception grpcInitEx)
            {
                var hostInitEx = new Exception($"Failed to start Rpc Server. Check if your app is hitting connection limits.", grpcInitEx);
            }
        }

        internal Task InitializeChannelsAsync()
        {
            _logger.LogInformation("Starting languageworker ");
            string workerId = Guid.NewGuid().ToString();
            _languageWorkerChannel = new LanguageWorkerChannel(workerId, _eventManager, _rpcServer.CSharpUri, _logger);
            _functionDispatcher.AddWorkerChannel(_languageWorkerChannel);
            _logger.LogInformation("done added to Function Dispatcher");
            return Task.CompletedTask;
        }
    }
}
