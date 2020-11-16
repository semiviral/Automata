using System;
using System.IO;
using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Context : OpenCLObject
    {
        public Context(CL cl, nint handle) : base(cl) => Handle = handle;

        public Program CreateProgram(string filePath, out int errorCode)
        {
            string[] lines = File.ReadAllLines(filePath);
            return CreateProgram(lines, out errorCode);
        }

        public Program CreateProgram(string[] lines, out int errorCode)
        {
            Span<nuint> lengths = stackalloc nuint[lines.Length];

            for (int index = 0; index < lines.Length; index++) lengths[index] = (nuint)lines[index].Length;

            Span<int> temp = stackalloc int[1];
            nint handle = CL.CreateProgramWithSource(Handle, (uint)lines.Length, lines, lengths, temp);
            errorCode = temp[0];

            return new Program(CL, handle);
        }
    }

    public record Program : OpenCLObject
    {
        internal Program(CL cl, nint handle) : base(cl) => Handle = handle;
    }
}
