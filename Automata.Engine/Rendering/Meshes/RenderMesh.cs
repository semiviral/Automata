#region

using System;
using System.Numerics;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : IComponentChangeable, IDisposable
    {
        private IMesh? _Mesh;

        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        public IMesh? Mesh
        {
            get => _Mesh;
            set
            {
                _Mesh = value;
                Changed = true;
            }
        }

        public bool Changed { get; set; }

        public bool ShouldRender => Mesh is not null && Mesh.Visible && Mesh.IndexesByteLength > 0;

        public void Dispose() => _Mesh?.Dispose();
    }
}
