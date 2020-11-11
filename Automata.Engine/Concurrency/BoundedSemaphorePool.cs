using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Automata.Engine.Concurrency
{
    public class BoundedSemaphorePool : Singleton<BoundedSemaphorePool>
    {
        private readonly CancellationTokenSource _CancellationTokenSource;
        private readonly ManualResetEventSlim _ModifyPoolReset;
        private SemaphoreSlim _Semaphore;
        private int _Size;

        public event EventHandler<Exception>? ExceptionOccurred;

        public CancellationToken CancellationToken => _CancellationTokenSource.Token;
        public int Size => _Size;

        public BoundedSemaphorePool()
        {
            _CancellationTokenSource = new CancellationTokenSource();
            _ModifyPoolReset = new ManualResetEventSlim(true);
            _Semaphore = new SemaphoreSlim(0);
        }

        public void Enqueue(Task task)
        {
            async Task Dispatch()
            {
                try
                {
                    if (_CancellationTokenSource.IsCancellationRequested) return;

                    await _Semaphore.WaitAsync(CancellationToken).ConfigureAwait(false);
                    await task.ConfigureAwait(false);
                    _Semaphore.Release(1);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    ExceptionOccurred?.Invoke(this, exception);
                }
            }

            // ensure the worker group isn't being modified
            _ModifyPoolReset.Wait(CancellationToken);

            if (Size == 0) throw new InvalidOperationException($"Pool is empty. Call {nameof(DefaultPoolSize)}() or {nameof(ModifyPoolSize)}().");
            else Task.Run(Dispatch, CancellationToken);
        }

        public void Enqueue(ValueTask valueTask)
        {
            async ValueTask Dispatch()
            {
                try
                {
                    if (_CancellationTokenSource.IsCancellationRequested) return;

                    await _Semaphore.WaitAsync(CancellationToken).ConfigureAwait(false);
                    await valueTask.ConfigureAwait(false);
                    _Semaphore.Release(1);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    ExceptionOccurred?.Invoke(this, exception);
                    Log.Information("ssss");
                }
            }

            // ensure the worker group isn't being modified
            _ModifyPoolReset.Wait(CancellationToken);

            if (Size == 0) throw new InvalidOperationException($"Pool is empty. Call {nameof(DefaultPoolSize)}() or {nameof(ModifyPoolSize)}().");
            else Task.Run(Dispatch, CancellationToken);
        }

        public void DefaultPoolSize() => ModifyPoolSize((uint)Math.Max(1, Environment.ProcessorCount - 2));

        public void ModifyPoolSize(uint size)
        {
            if ((size == Size) || CancellationToken.IsCancellationRequested) return;

            _ModifyPoolReset.Wait(CancellationToken);
            _ModifyPoolReset.Reset();

            if (Size > 0)
            {
                for (int i = 0; i < Size; i++) _Semaphore.Wait(CancellationToken);
                _Semaphore.Release(Size);
            }

            _Semaphore.Dispose();

            if (size > 0) _Semaphore = new SemaphoreSlim((int)size);

            Interlocked.Exchange(ref _Size, (int)size);
            _ModifyPoolReset.Set();
        }

        /// <summary>
        ///     Safely cancels work on the pool.
        /// </summary>
        public void Cancel() => _CancellationTokenSource.Cancel();

        /// <summary>
        ///     Aggressively abort all workers.
        /// </summary>
        /// <remarks>
        ///     Only use this method in emergency situations. Undefined behaviour occurs when threads stop executing abruptly.
        /// </remarks>
        /// <param name="abort">Whether or not to abort all workers in the worker group.</param>
        public void Abort(bool abort)
        {
            if (!abort) return;

            _ModifyPoolReset.Wait(CancellationToken);
            _ModifyPoolReset.Reset();

            ModifyPoolSize(0);

            // allow any errant waiters to fire so the pool can be cleaned up
            _ModifyPoolReset.Set();
        }

        ~BoundedSemaphorePool() => _Semaphore.Dispose();
    }
}
