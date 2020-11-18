using System;
using System.IO;
using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Context : OpenCLObject
    {
        internal Device Device { get; }

        internal Context(CL cl, Device device, nint handle) : base(cl)
        {
            Device = device;
            Handle = handle;
        }

        public Program CreateProgram(string filePath, out int errorCode)
        {
            string[] lines = File.ReadAllLines(filePath);
            return CreateProgram(lines, out errorCode);
        }

        public Program CreateProgram(string[] lines, out int errorCode)
        {
            Span<nuint> lengths = stackalloc nuint[lines.Length];

            for (int index = 0; index < lines.Length; index++)
            {
                lengths[index] = (nuint)lines[index].Length;
            }

            Span<int> temp = stackalloc int[1];
            nint handle = CL.CreateProgramWithSource(Handle, (uint)lines.Length, lines, lengths, temp);
            errorCode = temp[0];

            return new Program(CL, this, handle);
        }
    }
}
