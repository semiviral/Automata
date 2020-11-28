using System.Runtime.CompilerServices;

namespace Automata.Engine
{
    public delegate T? ResultCallback<in TResult, out T>(TResult result);

    public delegate void FailureCallback<in TFailure>(TFailure error);

    public delegate T? FailureCallbackWithReturn<in TFailure, out T>(TFailure error);

    public readonly ref struct Result<TResult, TFailure>
    {
        private readonly TResult? _Result;
        private readonly TFailure? _Error;

        public bool IsResult => _Result is not null;
        public bool IsError => _Error is not null;

        public Result(TResult? result)
        {
            _Result = result;
            _Error = default;
        }

        public Result(TFailure? error)
        {
            _Result = default;
            _Error = error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Match<T>(in ResultCallback<TResult, T> resultCallback, in FailureCallback<TFailure> failureCallback)
        {
            if (_Result is not null)
            {
                return resultCallback(_Result);
            }
            else if (_Error is not null)
            {
                failureCallback(_Error);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Match<T>(in ResultCallback<TResult, T> resultCallback, in FailureCallbackWithReturn<TFailure, T> failureCallback)
        {
            if (_Result is not null)
            {
                return resultCallback(_Result);
            }
            else if (_Error is not null)
            {
                return failureCallback(_Error);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Unwrap() => _Result!;


        #region Conversions

        public static implicit operator Result<TResult, TFailure>(TResult result) => new Result<TResult, TFailure>(result);
        public static implicit operator Result<TResult, TFailure>(TFailure failure) => new Result<TResult, TFailure>(failure);

        #endregion
    }
}
