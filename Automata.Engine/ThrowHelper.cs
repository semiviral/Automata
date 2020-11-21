using System;

namespace Automata.Engine
{
    public static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string? parameterName) => throw new ArgumentOutOfRangeException(parameterName);

        public static void ThrowArgumentOutOfRangeException(string? parameterName, string? message) =>
            throw new ArgumentOutOfRangeException(parameterName, message);

        public static void ThrowArgumentException(string? parameterName, string? message) => throw new ArgumentException(message, parameterName);
        public static void ThrowDestinationTooShort() => throw new ArgumentException("Destination too short.");

        public static void ThrowNullReferenceException(string? message) => throw new NullReferenceException(message);

        public static void ThrowInvalidOperationException(string? message) => throw new InvalidOperationException(message);

        public static void ThrowInsufficientMemoryException(string? message) => throw new InsufficientMemoryException(message);

        public static void ThrowIndexOutOfRangeException() =>
            throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the collection.");
    }
}
