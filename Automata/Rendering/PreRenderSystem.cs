#region

using Automata.Core;
using Automata.Core.Systems;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Pre-rendering system applies changed view matrices to their respective shaders.
    /// </summary>
    public class PreRenderSystem : ComponentSystem
    {
        public PreRenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(RenderedShader)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Camera camera, RenderedShader renderedShader) in entityManager.GetComponents<Camera, RenderedShader>())
            {
                renderedShader.Shader.SetUniform(nameof(camera.View), camera.View);
                renderedShader.Shader.SetUniform(nameof(camera.Projection), camera.Projection);
                renderedShader.Shader.SetUniform(nameof(camera.Model), camera.Model);
            }
        }
    }
}
