#region

using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Systems;
using Silk.NET.OpenGL;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

#endregion


namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private const bool _ENABLE_BACK_FACE_CULLING = false;
        private const bool _ENABLE_FRUSTUM_CULLING = true;

        private readonly GL _GL;
        private readonly UniformBufferObject _Viewport;

        private float _NewAspectRatio;

        public unsafe RenderSystem()
        {
            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;

            _GL = GLAPI.Instance.GL;
            _GL.ClearColor(Color.DimGray);
            _GL.Enable(EnableCap.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
                _GL.Enable(EnableCap.CullFace);
            }

            _Viewport = new UniformBufferObject(_GL, 1, (uint)sizeof(Vector4));
        }

        private void CheckUpdateViewportUBOAndBind()
        {
            if (_NewAspectRatio > 0f) _Viewport.Write(0, new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y));

            _Viewport.Bind();
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Camera), typeof(RenderMesh), typeof(Material))]
        public override unsafe ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];

            CheckUpdateViewportUBOAndBind();

            foreach ((IEntity cameraEntity, Camera camera) in entityManager.GetEntities<Camera>())
            {
                camera.Uniforms ??= new UniformBufferObject(_GL, 0, (uint)(sizeof(Matrix4x4) + sizeof(Matrix4x4) + sizeof(Vector4)))
                {
                    ["view"] = 0,
                    ["projection"] = 64,
                    ["parameters"] = 128
                };

                // check for changes and update current camera's view matrix & UBO data
                if ((cameraEntity.TryGetComponent(out Scale? cameraScale) && cameraScale.Changed)
                    | (cameraEntity.TryGetComponent(out Translation? cameraTranslation) && cameraTranslation.Changed)
                    | (cameraEntity.TryGetComponent(out Rotation? cameraRotation) && cameraRotation.Changed))
                {
                    camera.View = Matrix4x4.Identity;

                    if (cameraScale is not null) camera.View *= Matrix4x4.CreateScale(cameraScale.Value);
                    if (cameraRotation is not null) camera.View *= Matrix4x4.CreateFromQuaternion(cameraRotation.Value);
                    if (cameraTranslation is not null) camera.View *= Matrix4x4.CreateTranslation(cameraTranslation.Value);
                    if ((camera.View != Matrix4x4.Identity) && Matrix4x4.Invert(camera.View, out Matrix4x4 inverted)) camera.View = inverted;

                    camera.Uniforms.Write(0, camera.View);
                }

                // if the aspect ratio has changed, update the current camera's projection matrix & UBO data
                if (_NewAspectRatio > 0f)
                {
                    camera.Projection = camera.Projector switch
                    {
                        Projector.Perspective => new PerspectiveProjection(90f, _NewAspectRatio, 0.1f, 1000f),
                        Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                        Projector.None or _ => camera.Projection
                    };

                    camera.Uniforms.Write(64, camera.Projection!.Matrix);
                    camera.Uniforms.Write(128, camera.Projection!.Parameters);
                }

                // if the camera doesn't have a projection, it doesn't make sense to try and render to it
                if (camera.Projection is null) continue;

                // bind camera's view data UBO and precalculate viewproj matrix
                camera.Uniforms.Bind();
                Matrix4x4 viewProjection = camera.View * camera.Projection.Matrix;
                Material? cachedMaterial = null;

                // iterate each RenderMesh and check if the model matrix needs to be recalculated
                foreach ((IEntity objectEntity, RenderMesh renderMesh) in entityManager.GetEntities<RenderMesh>())
                    if (((objectEntity.TryGetComponent(out Scale? modelScale) && modelScale.Changed)
                         | (objectEntity.TryGetComponent(out Rotation? modelRotation) && modelRotation.Changed)
                         | (objectEntity.TryGetComponent(out Translation? modelTranslation) && modelTranslation.Changed))
                        || renderMesh.Changed)
                    {
                        renderMesh.Model = Matrix4x4.Identity;

                        if (modelTranslation is not null) renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation.Value);
                        if (modelRotation is not null) renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation.Value);
                        if (modelScale is not null) renderMesh.Model *= Matrix4x4.CreateScale(modelScale.Value);
                    }

                // iterate every valid entity and try to render it
                // we also sort the entities by their render pipeline ID, so we can avoid doing a ton of rebinding
                foreach ((IEntity objectEntity, RenderMesh renderMesh, Material material) in entityManager.GetEntities<RenderMesh, Material>()
                    .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                    .OrderBy(result => result.Component2.Pipeline.Handle))
                {
                    Matrix4x4 modelViewProjection = renderMesh.Model * viewProjection;

                    if (objectEntity.TryGetComponent(out OcclusionBounds? bounds) && CheckClipFrustumOcclude(bounds, planes, modelViewProjection)) continue;

                    // conditionally update the currentMaterial if it doesn't match this entity's
                    if (cachedMaterial is null || !material.Equals(cachedMaterial)) ApplyMaterial(material, ref cachedMaterial);

                    // we're about to render, so ensure all of the relevant uniforms are set
                    Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                    ShaderProgram vertexShader = cachedMaterial!.Pipeline.Stage(ShaderType.VertexShader);
                    vertexShader.TrySetUniform("_mvp", modelViewProjection);
                    vertexShader.TrySetUniform("_object", modelInverted);
                    vertexShader.TrySetUniform("_world", renderMesh.Model);

                    renderMesh.Mesh!.Bind();

                    _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh!.IndexesLength, DrawElementsType.UnsignedInt, (void*)null!);
                }
            }

            // by this point we should've updated all of the projection matrixes, so reset the value to 0.
            _NewAspectRatio = 0f;

            return ValueTask.CompletedTask;
        }

        private static bool CheckClipFrustumOcclude(OcclusionBounds occlusionBounds, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!_ENABLE_FRUSTUM_CULLING) return false;

            ClipFrustum frustum = new ClipFrustum(planes, mvp);
            Frustum.Intersect intersection = Frustum.Intersect.Outside;

            return

                // test spherical bounds
                ((occlusionBounds.Spheric != Sphere.Zero) && (intersection = frustum.Intersects(occlusionBounds.Spheric)) is Frustum.Intersect.Outside)

                // if spherical bounds occlusion fails (i.e. intersects) try cubic
                || (intersection is not Frustum.Intersect.Inside
                    && (occlusionBounds.Cubic != Cube.Zero)
                    && frustum.Intersects(occlusionBounds.Cubic) is Frustum.Intersect.Outside);
        }

        private static void ApplyMaterial(Material material, ref Material? old)
        {
            bool updatedPipeline = false;
            ShaderProgram? newFragmentShader = null;

            if (!material.Pipeline.Equals(old?.Pipeline))
            {
                material.Pipeline.Bind();

                // set fragment shader so we can easily bind textures
                newFragmentShader = material.Pipeline.Stage(ShaderType.FragmentShader);

                // if old isn't null, bind the old textures to the new pipeline
                if (old is not null)
                    for (int index = 0; index < old.Textures.Count; index++)
                    {
                        old.Textures[index].Bind(TextureUnit.Texture0 + index);
                        newFragmentShader.TrySetUniform($"_tex{index}", index);
                    }

                updatedPipeline = true;
            }

            // if newFragmentShader is null, then we didn't bind a new pipeline, so use old one
            newFragmentShader ??= old!.Pipeline.Stage(ShaderType.FragmentShader);
            int oldTextureCount = old?.Textures.Count ?? material.Textures.Count;
            bool updatedTextures = false;

            for (int index = 0; index < material.Textures.Count; index++)
            {
                // only update textures that are different
                if ((index < oldTextureCount) && material.Textures[index].Equals(old?.Textures[index])) continue;

                material.Textures[index].Bind(TextureUnit.Texture0 + index);
                newFragmentShader.TrySetUniform($"_tex{index}", index);

                // we've updated a texture, so set flag
                updatedTextures = true;
            }

            // if we've loaded any new material data, replace old material.
            if (updatedPipeline || updatedTextures) old = material;
        }

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;
    }
}
