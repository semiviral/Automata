namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : Component
    {
        public IMesh? Mesh { get; set; }

        public bool ShouldRender => Mesh?.Visible is true;


        #region IDisposable

        // IMesh'es probably contain native resources (GL objects), so dispose of it in the native cleanup
        protected override void CleanupNativeResources() => Mesh?.Dispose();

        #endregion
    }
}
