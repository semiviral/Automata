using System;

namespace Automata.Engine
{
    public static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string? parameterName) => throw new ArgumentOutOfRangeException(parameterName);

        public static void ThrowArgumentOutOfRangeException(string? parameterName, string? message) =>
            throw new ArgumentOutOfRangeException(parameterName, message);

        public static void ThrowNullReferenceException(string? message) => throw new NullReferenceException(message);

        public static void ThrowInvalidOperationException(string? message) => throw new InvalidOperationException(message);

        public static void ThrowInsufficientMemoryException(string? message) => throw new InsufficientMemoryException(message);
    }
}
