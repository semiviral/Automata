using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Device : OpenCLObject
    {
        public enum Type
        {
            All = -1,
            Default = 1,
            CPU = 2,
            GPU = 4,
            Accelerator = 8,
            Custom = 22
        }

        public Device(CL cl, nint handle) : base(cl) => Handle = handle;
    }
}
