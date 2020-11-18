using System;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : ComponentChangeable, IDisposable
    {
        private IMesh? _Mesh;

        public IMesh? Mesh
        {
            get => _Mesh;
            set
            {
                _Mesh = value;
                Changed = true;
            }
        }

        public bool ShouldRender => Mesh?.Visible is true;

        public void Dispose()
        {
            _Mesh?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
