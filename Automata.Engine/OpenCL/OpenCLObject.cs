using Silk.NET.OpenCL;

namespace Automata.Engine.OpenCL
{
    public abstract record OpenCLObject
    {
        protected readonly CL CL;

        public nint Handle { get; protected init; }

        public OpenCLObject(CL cl) => CL = cl;
    }
}
