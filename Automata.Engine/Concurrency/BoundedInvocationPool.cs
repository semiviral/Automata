using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Engine.Concurrency
{
    /// <summary>
    ///     Used to safely wrap a Task-returning method onto the <see cref="BoundedInvocationPool" />.
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" /> that the <see cref="BoundedInvocationPool" /> uses.
    /// </param>
    public delegate Task AsyncReferenceInvocation(CancellationToken cancellationToken);

    public class BoundedInvocationPool : Singleton<BoundedInvocationPool>
    {
        /// <summary>
        ///     Source for the <see cref="CancellationToken" /> that the pool observes.
        /// </summary>
        private readonly CancellationTokenSource _CancellationTokenSource;

        /// <summary>
        ///     Synchronization object used to protect the pool when modifications are being made to its size.
        /// </summary>
        private readonly ManualResetEventSlim _ModifyPoolReset;

        /// <summary>
        ///     Synchronization object used to control the level of concurrency.
        /// </summary>
        private SemaphoreSlim _Semaphore;

        /// <summary>
        ///     Current size of the pool (level of concurrency).
        /// </summary>
        private int _Size;

        /// <summary>
        ///     Invoked when an exception occurs inside a pool invocation.
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        /// <summary>
        ///     <see cref="CancellationToken" /> that is observed by internal methods.
        /// </summary>
        public CancellationToken CancellationToken => _CancellationTokenSource.Token;

        /// <summary>
        ///     Current size of the pool (level of concurrency).
        /// </summary>
        public int Size => _Size;

        public BoundedInvocationPool()
        {
            _CancellationTokenSource = new CancellationTokenSource();
            _ModifyPoolReset = new ManualResetEventSlim(true);
            _Semaphore = new SemaphoreSlim(0);
        }

        /// <summary>
        ///     Queues an invocation to the pool.
        /// </summary>
        /// <param name="invocation">Invocation to execute.</param>
        /// <exception cref="InvalidOperationException">Thrown when the pool size is 0.</exception>
        public void Enqueue(AsyncReferenceInvocation invocation)
        {
            // dispatch method used to wrap invocations
            // this allows us to observe cancellations, wait on the semaphore, and
            // generally control internal state before and after invocation execution.
            async Task dispatch_impl_impl()
            {
                try
                {
                    if (_CancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    await _Semaphore.WaitAsync(CancellationToken).ConfigureAwait(false);
                    await invocation.Invoke(CancellationToken).ConfigureAwait(false);
                    _Semaphore.Release(1);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // invoke exception event to propagate swallowed errors
                    ExceptionOccurred?.Invoke(this, exception);
                }
            }

            // ensure the pool size isn't being modified
            _ModifyPoolReset.Wait(CancellationToken);

            // ensure the pool is actually accepting invocations and dispatch
            if (Size == 0)
            {
                ThrowHelper.ThrowInvalidOperationException($"Pool is empty. Call {nameof(DefaultPoolSize)}() or {nameof(ModifyPoolSize)}().");
            }
            else
            {
                Task.Factory.StartNew(dispatch_impl_impl, CancellationToken);
            }
        }

        /// <summary>
        ///     Modifies pool size to <see cref="Environment.ProcessorCount" /> - 2.
        /// </summary>
        public void DefaultPoolSize() => ModifyPoolSize((uint)Math.Max(1, Environment.ProcessorCount - 2));

        /// <summary>
        ///     Safely locks synchronization object and modifies the pool size.
        /// </summary>
        /// <param name="size">New size for pool.</param>
        public void ModifyPoolSize(uint size)
        {
            // if size is already equal to new value, or we're cancelled, then return from method
            if ((size == Size) || CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // wait on modification synchronization object
            _ModifyPoolReset.Wait(CancellationToken);

            // reset object so waits will block
            _ModifyPoolReset.Reset();

            if (Size > 0)

                // if pool was accepting any invocations, wait until all
                // semaphore slots are freed (and thus, all invocation complete).
                //
                // we can be sure no NEW work will be added, since we locked the
                // modification synchronization object.
            {
                while (_Semaphore.CurrentCount < Size)
                {
                    Thread.Sleep(1);
                }
            }

            // dispose old semaphore
            _Semaphore.Dispose();

            // if size is greater than zero, we create a new semaphore
            // if not, then there's no point since it would be empty.
            if (size > 0)
            {
                _Semaphore = new SemaphoreSlim((int)size);
            }

            // atomic write new size value
            Interlocked.Exchange(ref _Size, (int)size);

            // set pool so dispatches can be queued again
            _ModifyPoolReset.Set();
        }

        /// <summary>
        ///     Safely cancels work on the pool.
        /// </summary>
        public void Cancel() => _CancellationTokenSource.Cancel();

        /// <summary>
        ///     Aggressively abort all invocations.
        /// </summary>
        /// <remarks>
        ///     Only use this method in emergency situations. Undefined behaviour occurs.
        /// </remarks>
        /// <param name="abort">Whether or not to abort all invocations.</param>
        public void Abort(bool abort)
        {
            if (!abort)
            {
                return;
            }

            _CancellationTokenSource.Cancel();

            ModifyPoolSize(0);
        }

        ~BoundedInvocationPool() => _Semaphore.Dispose();
    }
}
