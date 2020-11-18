using System;
using Silk.NET.OpenCL;

namespace Automata.Engine.OpenCL
{
    public class CLAPI : Singleton<CLAPI>
    {
        public CL CL { get; }

        public CLAPI() => CL = CL.GetApi();

        public static Platform[] GetPlatforms() => GetPlatforms(Instance.CL);

        public static unsafe Platform[] GetPlatforms(CL cl)
        {
            uint platformCount = 0u;
            cl.GetPlatformIDs(0u, (nint*)null!, &platformCount);

            Span<nint> handles = stackalloc nint[(int)platformCount];
            cl.GetPlatformIDs(platformCount, handles, null);

            Platform[] platforms = new Platform[platformCount];

            for (int index = 0; index < platformCount; index++)
            {
                platforms[index] = new Platform(cl, handles[index]);
            }

            return platforms;
        }
    }
}
