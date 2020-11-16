using System;
using System.Text;
using Automata.Playground.OpenCL;
using Silk.NET.OpenCL;

CL cl;
Main();

unsafe void Main()
{
    cl = CLAPI.Instance.CL;

    Platform platform = CLAPI.GetPlatforms(cl)[0];
    Device device = platform.GetDevices(DeviceType.All)[0];

    string[] sourceStrings =
    {
        " __kernel void parallel_add(__global float* x, __global float* y, __global float* z){\r\n",
        " const int i = get_global_id(0);\r\n", // get a unique number identifying the work item in the global pool
        " z[i] = y[i] + x[i];\r\n", // add two arrays
        "}\r\n"
    };

    Context context = device.CreateContext(Span<nint>.Empty, null);
    Program program = context.CreateProgram(sourceStrings, out _);
    program.Build(string.Empty, null);
}
