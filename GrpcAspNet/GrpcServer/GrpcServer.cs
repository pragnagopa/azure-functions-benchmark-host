// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using TestGrpc.Messages;

namespace GrpcServer
{
    public class GrpcServer : IRpcServer, IDisposable
    {
        private Server _server;
        const int Port = 50052;
        private bool _disposed = false;
        public const int MaxMessageLengthBytes = 128 * 1024 * 1024;

        public GrpcServer(FunctionRpc.FunctionRpcBase serviceImpl)
        {
            ChannelOption maxReceiveMessageLength = new ChannelOption(ChannelOptions.MaxReceiveMessageLength, MaxMessageLengthBytes);
            ChannelOption maxSendMessageLength = new ChannelOption(ChannelOptions.MaxSendMessageLength, MaxMessageLengthBytes);
            ChannelOption[] grpcChannelOptions = { maxReceiveMessageLength, maxSendMessageLength };
            _server = new Server(grpcChannelOptions)
            {
                Services = { FunctionRpc.BindService(serviceImpl) },
                Ports = { new ServerPort("127.0.0.1", Port, ServerCredentials.Insecure) }
            };
        }

        public Uri Uri => new Uri($"http://127.0.0.1:{Port}");

        public string CSharpUri => $"127.0.0.1:{Port}";

        public Task StartAsync()
        {
            _server.Start();
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => _server.ShutdownAsync();

        public Task KillAsync() => _server.KillAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _server.ShutdownAsync();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}