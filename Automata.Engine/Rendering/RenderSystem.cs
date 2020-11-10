#region

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
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

            foreach (IEntity cameraEntity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = cameraEntity.GetComponent<Camera>();

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
                Material? currentMaterial = null;

                foreach (IEntity objectEntity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                {
                    RenderMesh renderMesh = objectEntity.GetComponent<RenderMesh>();

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

                    Matrix4x4 modelViewProjection = renderMesh.Model * viewProjection;

                    if (!renderMesh.ShouldRender // check if should render at all
                        || ((camera.RenderedLayers & renderMesh.Mesh!.Layer) != renderMesh.Mesh!.Layer)
                        || !objectEntity.TryGetComponent(out Material? material)

                        // check if occluded by frustum
                        || (objectEntity.TryGetComponent(out OcclusionBounds? bounds)
                            && _ENABLE_FRUSTUM_CULLING
                            && CheckClipFrustumOcclude(bounds, planes, modelViewProjection))) continue;

                    if (currentMaterial is null || (material.Pipeline.Handle != currentMaterial.Pipeline.Handle))
                    {
                        currentMaterial = material;
                        currentMaterial.Pipeline.Bind();
                        ShaderProgram fragmentShader = currentMaterial.Pipeline.Stage(ShaderType.FragmentShader);

                        for (int index = 0; index < currentMaterial.Textures.Length; index++)
                        {
                            currentMaterial.Textures[index]?.Bind(TextureUnit.Texture0 + index);
                            fragmentShader.TrySetUniform($"_tex{index}", index);
                        }
                    }

                    Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                    currentMaterial.Pipeline.Stage(ShaderType.VertexShader).TrySetUniform("_mvp", modelViewProjection);
                    currentMaterial.Pipeline.Stage(ShaderType.VertexShader).TrySetUniform("_object", modelInverted);
                    currentMaterial.Pipeline.Stage(ShaderType.VertexShader).TrySetUniform("_world", renderMesh.Model);

                    renderMesh.Mesh!.Bind();

                    _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh!.IndexesLength, DrawElementsType.UnsignedInt, null);
                }
            }

            GLAPI.UnbindVertexArray();
            _NewAspectRatio = 0f;
        }

        private static bool CheckClipFrustumOcclude(OcclusionBounds occlusionBounds, Span<Plane> planes, Matrix4x4 mvp)
        {
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

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;
    }
}
