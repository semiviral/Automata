using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan
{
    public sealed class SurfaceExtension : KhrSurface
    {
        public SurfaceExtension(INativeContext nativeContext) : base(nativeContext) { }
    }
}
