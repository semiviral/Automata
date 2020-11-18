using System.Numerics;

namespace Automata.Engine.Rendering
{
    public interface IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }
    }
}
