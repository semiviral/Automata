#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
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
        private readonly UniformBuffer _Matrixes;

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

            _Matrixes = new UniformBuffer(_GL, 0, (uint)sizeof(Matrix4x4) * 3u);
        }

        [HandlesComponents(DistinctionStrategy.Any, typeof(Camera), typeof(RenderMesh))]
        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            _Matrixes.Bind();

            Vector4 viewport = new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];

            foreach (IEntity cameraEntity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = cameraEntity.GetComponent<Camera>();

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
                }

                if (_NewAspectRatio > 0f)
                {
                    camera.Projection = camera.Projector switch
                    {
                        Projector.Perspective => new PerspectiveProjection(90f, _NewAspectRatio, 0.1f, 1000f),
                        Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                        Projector.None or _ => camera.Projection
                    };
                }

                Matrix4x4 viewProjection = (camera.View * camera.Projection?.Matrix) ?? Matrix4x4.Identity;
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
                    _Matrixes.Write(0, modelViewProjection);

                    if (!renderMesh.ShouldRender // check if should render at all
                        || ((camera.RenderedLayers & renderMesh.Mesh!.Layer) != renderMesh.Mesh!.Layer)
                        || !objectEntity.TryGetComponent(out Material? material)

                        // check if occluded by frustum
                        || (objectEntity.TryGetComponent(out OcclusionBounds? bounds)
                            && _ENABLE_FRUSTUM_CULLING
                            && CheckClipFrustumOcclude(bounds, planes, modelViewProjection))) continue;

                    if (currentMaterial is null || (material.Pipeline.Handle != currentMaterial.Pipeline.Handle))
                    {
                        material.Pipeline.Bind();

                        for (int index = 0; index < material.Textures.Length; index++)
                        {
                            material.Textures[index]?.Bind(TextureUnit.Texture0 + index);
                            material.Pipeline.Stage(ShaderType.FragmentShader).TrySetUniform($"_tex{index}", index);
                        }

                        currentMaterial = material;
                    }

                    Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                    _Matrixes.Write(64, modelInverted); // object
                    _Matrixes.Write(128, renderMesh.Model); // world

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
