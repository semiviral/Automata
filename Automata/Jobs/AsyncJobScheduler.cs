#region

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Automata.Jobs
{
    public static class AsyncJobScheduler
    {
        private static readonly CancellationTokenSource _abortTokenSource;
        private static readonly SemaphoreSlim _workerSemaphore;
        private static long _QueuedJobs;
        private static long _ProcessingJobs;

        public static CancellationToken AbortToken => _abortTokenSource.Token;
        public static long QueuedJobs => Interlocked.Read(ref _QueuedJobs);
        public static long ProcessingJobs => Interlocked.Read(ref _ProcessingJobs);

        public static int MaximumProcessingJobs { get; }

        /// <summary>
        ///     Initializes a new instance of <see cref="AsyncJobScheduler" /> class.
        /// </summary>
        static AsyncJobScheduler()
        {
            _abortTokenSource = new CancellationTokenSource();

            MaximumProcessingJobs = Environment.ProcessorCount - 2;

            JobQueued += (sender, args) =>
            {
                Interlocked.Increment(ref _QueuedJobs);

                return Task.CompletedTask;
            };
            JobStarted += (sender, args) =>
            {
                Interlocked.Decrement(ref _QueuedJobs);
                Interlocked.Increment(ref _ProcessingJobs);

                return Task.CompletedTask;
            };
            JobFinished += (sender, args) =>
            {
                Interlocked.Decrement(ref _ProcessingJobs);

                return Task.CompletedTask;
            };

            _workerSemaphore = new SemaphoreSlim(MaximumProcessingJobs);
        }


        #region State

        /// <summary>
        ///     Aborts execution of job scheduler.
        /// </summary>
        public static void Abort(bool abort)
        {
            if (abort)
            {
                _abortTokenSource.Cancel();
            }
        }

        public static void QueueAsyncJob(AsyncJob asyncJob)
        {
            if (AbortToken.IsCancellationRequested)
            {
                return;
            }

            OnJobQueued(new AsyncJobEventArgs(asyncJob));

            Task.Run(() => ExecuteJob(asyncJob));
        }

        #endregion


        #region Runtime

        private static async Task ExecuteJob(AsyncJob asyncJob)
        {
            if (_abortTokenSource.IsCancellationRequested)
            {
                return;
            }

            await _workerSemaphore.WaitAsync().ConfigureAwait(false);

            OnJobStarted(new AsyncJobEventArgs(asyncJob));

            await asyncJob.Execute().ConfigureAwait(false);

            OnJobFinished(new AsyncJobEventArgs(asyncJob));

            _workerSemaphore.Release();
        }

        #endregion


        #region Events

        /// <summary>
        ///     Called when a job is queued.
        /// </summary>
        /// <remarks>This event will not necessarily happen synchronously with the main thread.</remarks>
        public static event AsyncJobEventHandler JobQueued;

        /// <summary>
        ///     Called when a job starts execution.
        /// </summary>
        /// <remarks>This event will not necessarily happen synchronously with the main thread.</remarks>
        public static event AsyncJobEventHandler JobStarted;

        /// <summary>
        ///     Called when a job finishes execution.
        /// </summary>
        /// <remarks>This event will not necessarily happen synchronously with the main thread.</remarks>
        public static event AsyncJobEventHandler JobFinished;


        private static void OnJobQueued(AsyncJobEventArgs args)
        {
            JobQueued?.Invoke(JobQueued, args);
        }

        private static void OnJobStarted(AsyncJobEventArgs args)
        {
            JobStarted?.Invoke(JobStarted, args);
        }

        private static void OnJobFinished(AsyncJobEventArgs args)
        {
            JobFinished?.Invoke(JobFinished, args);
        }

        #endregion
    }
}
