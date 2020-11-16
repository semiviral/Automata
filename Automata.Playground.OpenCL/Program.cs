using System;
using Automata.Playground.OpenCL;
using Silk.NET.OpenCL;

CL cl;
Main();

void Main()
{
    cl = CLAPI.Instance.CL;

    Platform platform = CLAPI.GetPlatforms(cl)[0];
    Device device = platform.GetDevices(DeviceType.All)[0];

    string[] source_strings =
    {
        " __kernel void parallel_add(__global float* x, __global float* y, __global float* z){\r\n",
        " const int i = get_global_id(0);\r\n", // get a unique number identifying the work item in the global pool
        " z[i] = y[i] + x[i];\r\n", // add two arrays
        "}\r\n"
    };

    cl.Program
}

namespace Automata.Playground.OpenCL
{
    public record Program : OpenCLObject
    {
        internal Program(CL cl, nint handle, Span<string> sourceLines) : base(cl)
        {
            Handle = handle;
            CL.CreateProgramWithSource()
        }

        public
    }
}
