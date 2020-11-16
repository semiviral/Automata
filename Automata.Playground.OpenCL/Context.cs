using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Context : OpenCLObject
    {
        public Context(CL cl, nint handle) : base(cl) => Handle = handle;
    }
}
