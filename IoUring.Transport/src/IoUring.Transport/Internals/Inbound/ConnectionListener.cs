using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace IoUring.Transport.Internals.Inbound
{
    internal class ConnectionListener : IConnectionListener
    {
        private readonly IoUringTransport _transport;
        private readonly IoUringOptions _options;

        private ChannelReader<ConnectionContext> _acceptQueue;

        private ConnectionListener(EndPoint endpoint, IoUringTransport transport, IoUringOptions options)
        {
            EndPoint = endpoint;
            _transport = transport;
            _options = options;
        }

        public EndPoint EndPoint { get; }

        public static ValueTask<IConnectionListener> Create(EndPoint endpoint, IoUringTransport transport, IoUringOptions options)
        {
            var listener = new ConnectionListener(endpoint, transport, options);
            listener.Bind();
            return new ValueTask<IConnectionListener>(listener);
        }

        private void Bind()
        {
            if (!(EndPoint is IPEndPoint)) throw new NotSupportedException();
            if (EndPoint.AddressFamily != AddressFamily.InterNetwork && EndPoint.AddressFamily != AddressFamily.InterNetworkV6) throw new NotSupportedException();

            var acceptQueue = Channel.CreateUnbounded<ConnectionContext>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = _options.ApplicationSchedulingMode == PipeScheduler.Inline
            });
            _acceptQueue = acceptQueue.Reader;

            var threads = _transport.TransportThreads;
            foreach (var thread in threads)
            {
                thread.Bind((IPEndPoint) EndPoint, acceptQueue.Writer);
            }
        }

        public async ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var connection in _acceptQueue.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                return connection;
            }

            return null;
        }

        public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}