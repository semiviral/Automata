using Silk.NET.OpenCL;

namespace Automata.Playground.OpenCL
{
    internal class Program
    {
        private static CL _CL;

        private static void Main(string[] args)
        {
            _CL = CLAPI.Instance.CL;

            Platform platform = CLAPI.GetPlatforms(_CL)[0];
            Device[] devices = platform.GetDevices(DeviceType.All);
        }
    }
}
