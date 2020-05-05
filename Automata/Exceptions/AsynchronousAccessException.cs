#region

using System;

#endregion

namespace Automata.Exceptions
{
    public class AsynchronousAccessException : Exception
    {
        public AsynchronousAccessException(string message) : base(message) { }
    }
}
