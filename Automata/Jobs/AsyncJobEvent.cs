#region

using System;
using System.Threading.Tasks;

#endregion

namespace Automata.Jobs
{
    public delegate Task AsyncJobEventHandler(object sender, AsyncJobEventArgs args);

    public class AsyncJobEventArgs : EventArgs
    {
        public readonly AsyncJob AsyncJob;

        public AsyncJobEventArgs(AsyncJob asyncJob) => AsyncJob = asyncJob;
    }
}
