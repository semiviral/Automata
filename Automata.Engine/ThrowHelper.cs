using System;

namespace Automata.Engine
{
    public static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string? parameterName) => throw new ArgumentOutOfRangeException(parameterName);
    }
}
