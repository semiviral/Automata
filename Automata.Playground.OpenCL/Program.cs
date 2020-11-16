using System;
using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Program : OpenCLObject
    {
        internal Context Context { get; }

        internal Program(CL cl, Context context, nint handle) : base(cl)
        {
            Context = context;
            Handle = handle;
        }

        public void Build(string options, NotifyCallback? notifyCallback) => Build(options, notifyCallback, Span<byte>.Empty);

        public void Build<T>(string options, NotifyCallback? notifyCallback, Span<T> userData) where T : unmanaged =>
            CL.BuildProgram(Handle, 1u, stackalloc[]
            {
                Context.Device.Handle
            }, options, notifyCallback, userData);
    }
}
