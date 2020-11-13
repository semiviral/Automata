using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Automata.Engine.Collections
{
    public class ConcurrentChannel<T>
    {
        private readonly ChannelReader<T> _Reader;
        private readonly ChannelWriter<T> _Writer;

        private long _Count;

        public long Count => Interlocked.Read(ref _Count);

        public ConcurrentChannel(bool singleReader, bool singleWriter)
        {
            Channel<T> channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = singleReader,
                SingleWriter = singleWriter
            });

            _Reader = channel.Reader;
            _Writer = channel.Writer;
        }

        public bool TryAdd(T item)
        {
            if (_Writer.TryWrite(item))
            {
                Interlocked.Increment(ref _Count);
                return true;
            }
            else return false;
        }

        public bool TryTake(out T item)
        {
            if (_Reader.TryRead(out item))
            {
                Interlocked.Decrement(ref _Count);
                return true;
            }
            else return false;
        }

        public async ValueTask AddAsync(T item, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _Count);
            await _Writer.WriteAsync(item, cancellationToken);
        }

        public async ValueTask<T> TakeAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Decrement(ref _Count);
            return await _Reader.ReadAsync(cancellationToken);
        }
    }
}
