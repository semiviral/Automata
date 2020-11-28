using System;
using System.Runtime.CompilerServices;

namespace Automata.Engine
{
    public delegate void ResultCallback<in TResult>(TResult result);

    public delegate T ResultCallback<in TResult, out T>(TResult result);

    public delegate void ErrorCallback<in TFailure>(TFailure error);

    public delegate T ErrorCallback<in TFailure, out T>(TFailure error);

    public readonly ref struct Result<TResult, TError>
    {
        private readonly TResult? _Result;
        private readonly TError? _Error;

        public bool IsResult => _Result is not null;
        public bool IsError => _Error is not null;

        public Result(TResult? result)
        {
            _Result = result;
            _Error = default;
        }

        public Result(TError? error)
        {
            _Result = default;
            _Error = error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Match<T>(in ResultCallback<TResult, T> resultCallback, in ErrorCallback<TError> errorCallback)
        {
            if (_Result is not null)
            {
                return resultCallback(_Result);
            }
            else if (_Error is not null)
            {
                errorCallback(_Error);
            }

            return default!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Match<T>(in ResultCallback<TResult, T> resultCallback, in ErrorCallback<TError, T> errorCallback)
        {
            if (_Result is not null)
            {
                return resultCallback(_Result);
            }
            else if (_Error is not null)
            {
                return errorCallback(_Error);
            }

            return default!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(in ResultCallback<TResult> resultCallback, in ErrorCallback<TError> errorCallback)
        {
            if (_Result is not null)
            {
                resultCallback(_Result);
            }
            else if (_Error is not null)
            {
                errorCallback(_Error);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Unwrap() => _Result!;

        public TResult Expect(string message) => _Result ?? throw new InvalidOperationException(message);
        public TError ExpectError(string message) => _Error ?? throw new InvalidOperationException(message);
        public TResult Default(TResult value) => _Result ?? value;


        #region Conversions

        public static implicit operator Result<TResult, TError>(TResult result) => new Result<TResult, TError>(result);
        public static implicit operator Result<TResult, TError>(TError failure) => new Result<TResult, TError>(failure);

        #endregion
    }
}
