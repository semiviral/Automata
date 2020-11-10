#region

using System;
using System.Linq;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.GLFW;
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
        private readonly UniformBuffer _Viewport;

        private float _NewAspectRatio;

        public unsafe RenderSystem()
        {
            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;

            _GL = GLAPI.Instance.GL;
            _GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
            _GL.Enable(GLEnum.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
                _GL.Enable(GLEnum.CullFace);
            }

            _Viewport = new UniformBuffer(_GL, 1, (uint)sizeof(Vector4));
        }

        [HandlesComponents(DistinctionStrategy.Any, typeof(Camera), typeof(RenderMesh))]
        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];

            if (_NewAspectRatio > 0f) _Viewport.Write(0, new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y));

            _Viewport.Bind();

            foreach ((IEntity cameraEntity, Camera camera) in entityManager.GetEntities<Camera>())
            {
                camera.Uniforms ??= new UniformBuffer(_GL, 0, (uint)(sizeof(Matrix4x4) + sizeof(Matrix4x4) + sizeof(Vector4)))
                {
                    ["view"] = 0,
                    ["projection"] = 64,
                    ["parameters"] = 128
                };

                if ((cameraEntity.TryGetComponent(out Scale? cameraScale) && cameraScale.Changed)
                    | (cameraEntity.TryGetComponent(out Translation? cameraTranslation) && cameraTranslation.Changed)
                    | (cameraEntity.TryGetComponent(out Rotation? cameraRotation) && cameraRotation.Changed))
                {
                    camera.View = Matrix4x4.Identity;
                    camera.View *= Matrix4x4.CreateScale(cameraScale?.Value ?? Scale.DEFAULT);
                    camera.View *= Matrix4x4.CreateFromQuaternion(cameraRotation?.Value ?? Quaternion.Identity);
                    camera.View *= Matrix4x4.CreateTranslation(cameraTranslation?.Value ?? Vector3.Zero);

                    Matrix4x4.Invert(camera.View, out Matrix4x4 inverted);
                    camera.View = inverted;
                    camera.Uniforms.Write(0, camera.View);
                }

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

                if (camera.Projection is null) continue;

                camera.Uniforms.Bind();
                Matrix4x4 viewProjection = camera.View * camera.Projection.Matrix;
                Material? cachedMaterial = null;

                foreach ((IEntity objectEntity, RenderMesh renderMesh) in entityManager.GetEntities<RenderMesh>())
                {
                    if (((objectEntity.TryGetComponent(out Scale? modelScale) && modelScale.Changed)
                         | (objectEntity.TryGetComponent(out Rotation? modelRotation) && modelRotation.Changed)
                         | (objectEntity.TryGetComponent(out Translation? modelTranslation) && modelTranslation.Changed))
                        || renderMesh.Changed)
                    {
                        renderMesh.Model = Matrix4x4.Identity;
                        renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation?.Value ?? Vector3.Zero);
                        renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation?.Value ?? Quaternion.Identity);
                        renderMesh.Model *= Matrix4x4.CreateScale(modelScale?.Value ?? Scale.DEFAULT);
                    }
                }

                foreach ((IEntity objectEntity, RenderMesh renderMesh, Material material) in entityManager.GetEntities<RenderMesh, Material>()
                    .OrderBy(result => result.Component2.Pipeline.Handle))
                {
                    if (!renderMesh.ShouldRender || ((camera.RenderedLayers & renderMesh.Mesh!.Layer) != renderMesh.Mesh!.Layer)) continue;

                    Matrix4x4 modelViewProjection = renderMesh.Model * viewProjection;

                    if (objectEntity.TryGetComponent(out OcclusionBounds? bounds) && CheckClipFrustumOcclude(bounds, planes, modelViewProjection)) continue;

                    if (cachedMaterial is null || !material.Equals(cachedMaterial)) ApplyMaterial(material, ref cachedMaterial);

                    Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                    ShaderProgram vertexShader = cachedMaterial!.Pipeline.Stage(ShaderType.VertexShader);
                    vertexShader.TrySetUniform("_mvp", modelViewProjection);
                    vertexShader.TrySetUniform("_object", modelInverted);
                    vertexShader.TrySetUniform("_world", renderMesh.Model);

                    renderMesh.Mesh!.Bind();

                    _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh!.IndexesLength, DrawElementsType.UnsignedInt, (void*)null!);
                }
            }

            GLAPI.UnbindVertexArray();
            _NewAspectRatio = 0f;
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
            // store boolean value for use later
            bool updatePipeline = !material.Pipeline.Equals(old?.Pipeline);
            ShaderProgram? newFragmentShader = null;

            if (updatePipeline)
            {
                // bind new pipeline
                material.Pipeline.Bind();
                // set fragment shader so we can easily bind textures
                newFragmentShader = material.Pipeline.Stage(ShaderType.FragmentShader);

                // if old isn't null, bind the old textures to the new pipeline
                if (old is not null)
                {
                    for (int index = 0; index < old.Textures.Count; index++)
                    {
                        old.Textures[index].Bind(TextureUnit.Texture0 + index);
                        newFragmentShader.TrySetUniform($"_tex{index}", index);
                    }
                }
            }

            // if newFragmentShader is null, then we didn't bind a new pipeline
            newFragmentShader ??= old!.Pipeline.Stage(ShaderType.FragmentShader);
            // cache the old texture count, or material count if old is null
            int oldTextureCount = old?.Textures.Count ?? material.Textures.Count;
            // this bool indicates whether we updated any textures
            bool updateTextures = false;

            for (int index = 0; index < material.Textures.Count; index++)
            {
                // if textures match, continue to next
                if (index < oldTextureCount && material.Textures[index].Equals(old?.Textures[index])) continue;

                // textures don't match, so reassign this specific texture channel
                material.Textures[index].Bind(TextureUnit.Texture0 + index);
                newFragmentShader.TrySetUniform($"_tex{index}", index);
                // we've updated a texture, so set boolean value
                updateTextures = true;
            }

            // if we've loaded any new material data, replace old material.
            if (updatePipeline || updateTextures) old = material;
        }

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;
    }
}
