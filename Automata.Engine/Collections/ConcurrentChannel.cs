using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Automata.Engine.Collections
{
    public class ConcurrentChannel<T> : IDisposable
    {
        private readonly Channel<T> _Channel;
        private readonly ChannelReader<T> _Reader;
        private readonly ChannelWriter<T> _Writer;

        public ConcurrentChannel(bool singleReader, bool singleWriter)
        {
            _Channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = singleReader,
                SingleWriter = singleWriter
            });

            _Reader = _Channel.Reader;
            _Writer = _Channel.Writer;
        }

        public bool TryAdd(T item) => _Writer.TryWrite(item);
        public bool TryTake([MaybeNullWhen(false)] out T item) => _Reader.TryRead(out item);
        public async ValueTask AddAsync(T item, CancellationToken cancellationToken = default) => await _Writer.WriteAsync(item, cancellationToken);
        public async ValueTask<T> TakeAsync(CancellationToken cancellationToken = default) => await _Reader.ReadAsync(cancellationToken);


        #region IDisposable

        public void Dispose()
        {
            // empty channel of items

            _Channel.Writer.Complete();

            while (TryTake(out _)) { }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
