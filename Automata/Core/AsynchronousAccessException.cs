#region

using System;

#endregion

namespace Automata.Core
{
    public class AsynchronousAccessException : Exception
    {
        public AsynchronousAccessException(string message) : base(message) { }
    }
}
