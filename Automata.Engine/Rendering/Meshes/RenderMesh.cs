namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : ComponentChangeable
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


        #region IDisposable

        protected override void CleanupManagedResources() => _Mesh?.Dispose();

        #endregion
    }
}
