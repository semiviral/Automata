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

        // IMesh'es probably contain native resources (GL objects), so dispose of it in the native cleanup
        protected override void CleanupNativeResources() => _Mesh?.Dispose();

        #endregion
    }
}
