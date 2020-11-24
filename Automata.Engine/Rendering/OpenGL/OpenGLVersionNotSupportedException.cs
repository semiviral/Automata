using System;
using Silk.NET.Windowing;

namespace Automata.Engine.Rendering.OpenGL
{
    public class OpenGLVersionNotSupportedException : Exception
    {
        public APIVersion PreferredVersion { get; }

        public OpenGLVersionNotSupportedException(APIVersion preferredVersion)
            : base($"Preferred OpenGL version {preferredVersion.MajorVersion}.{preferredVersion.MinorVersion} is not supported.")
            => PreferredVersion = preferredVersion;
    }
}
