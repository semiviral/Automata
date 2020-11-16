using System;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    public record Platform : OpenCLObject
    {
        private enum Parameter : uint
        {
            Profile = 2304u, // 0x00000900
            Version = 2305u, // 0x00000901
            Name = 2306u, // 0x00000902
            Vendor = 2307u, // 0x00000903
            Extensions = 2308u, // 0x00000904
            HostTimerResolution = 2309u // 0x00000905
        }

        public string Profile { get; }
        public string Version { get; }
        public string Name { get; }
        public string Vendor { get; }
        public ulong HostTimerResolution { get; }
        public string[] Extensions { get; }

        public Platform(CL cl, nint handle) : base(cl)
        {
            Handle = handle;
            Profile = Encoding.ASCII.GetString(GetInfo(Parameter.Profile)[..^1]);
            Version = Encoding.ASCII.GetString(GetInfo(Parameter.Version)[..^1]);
            Name = Encoding.ASCII.GetString(GetInfo(Parameter.Name)[..^1]);
            Vendor = Encoding.ASCII.GetString(GetInfo(Parameter.Vendor)[..^1]);
            Extensions = Encoding.ASCII.GetString(GetInfo(Parameter.Extensions)[..^1]).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HostTimerResolution = MemoryMarshal.Read<ulong>(GetInfo(Parameter.HostTimerResolution));
        }

        private unsafe Span<byte> GetInfo(Parameter parameter)
        {
            nuint returnSize = 0u;
            CL.GetPlatformInfo(Handle, (uint)parameter, (nuint)0u, (void*)null!, &returnSize);
            Span<byte> value = new byte[(uint)returnSize];
            CL.GetPlatformInfo(Handle, (uint)parameter, returnSize, value, null!);
            return value;
        }

        public unsafe Device[] GetDevices(DeviceType deviceType)
        {
            uint deviceCount = 0u;
            CL.GetDeviceIDs(Handle, (CLEnum)deviceType, 0u, (nint*)null!, &deviceCount);

            Span<nint> handles = stackalloc nint[(int)deviceCount];
            CL.GetDeviceIDs(Handle, (CLEnum)deviceType, deviceCount, handles, null);

            Device[] devices = new Device[deviceCount];
            for (int index = 0; index < devices.Length; index++) devices[index] = new Device(CL, handles[index]);
            return devices;
        }
    }
}
