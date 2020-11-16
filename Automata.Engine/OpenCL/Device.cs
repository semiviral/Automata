using System;
using System.Text;
using Silk.NET.OpenCL;

namespace Automata.Engine.OpenCL
{
    public record Device : OpenCLObject
    {
        private enum Parameter
        {
            Name = 4139, // 0x0000102B
            Vendor = 4140, // 0x0000102C
            DriverVersion = 4141, // 0x0000102D
            Profile = 4142, // 0x0000102E
            Version = 4143, // 0x0000102F
            Extensions = 4144 // 0x00001030
        }

        public string Profile { get; }
        public string Version { get; }
        public string DriverVersion { get; }
        public string Name { get; }
        public string Vendor { get; }
        public string[] Extensions { get; }

        public Device(CL cl, nint handle) : base(cl)
        {
            Handle = handle;
            Profile = Encoding.ASCII.GetString(GetInfo(Parameter.Profile)[..^1]);
            Version = Encoding.ASCII.GetString(GetInfo(Parameter.Version)[..^1]);
            DriverVersion = Encoding.ASCII.GetString(GetInfo(Parameter.DriverVersion)[..^1]);
            Name = Encoding.ASCII.GetString(GetInfo(Parameter.Name)[..^1]);
            Vendor = Encoding.ASCII.GetString(GetInfo(Parameter.Vendor)[..^1]);
            Extensions = Encoding.ASCII.GetString(GetInfo(Parameter.Extensions)[..^1]).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private unsafe Span<byte> GetInfo(Parameter parameter)
        {
            nuint returnSize = 0u;
            CL.GetDeviceInfo(Handle, (uint)parameter, (nuint)0u, (void*)null!, &returnSize);
            Span<byte> value = new byte[(uint)returnSize];
            CL.GetDeviceInfo(Handle, (uint)parameter, returnSize, value, null!);
            return value;
        }

        public Context CreateContext(Span<nint> properties, NotifyCallback? notifyCallback) => CreateContext(properties, notifyCallback, Span<byte>.Empty);

        // todo abstract a ContextProperties record or something to pass in
        public unsafe Context CreateContext<T>(Span<nint> properties, NotifyCallback? notifyCallback, Span<T> userData) where T : unmanaged =>
            new Context(CL, this, CL.CreateContext(properties, 1u, stackalloc[]
            {
                Handle
            }, notifyCallback, userData, null));
    }
}
