#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class Material : Component, IEquatable<Material>
    {
        public ProgramPipeline Pipeline { get; set; }
        public List<Texture> Textures { get; }

        public Material(ProgramPipeline pipeline) => (Pipeline, Textures) = (pipeline, new List<Texture>());

        public bool Equals(Material? other) => other is not null && Pipeline.Equals(other.Pipeline) && Textures.SequenceEqual(other.Textures);
        public override bool Equals(object? obj) => obj is Material material && Equals(material);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Pipeline, Textures);

        public static bool operator ==(Material? left, Material? right) => Equals(left, right);
        public static bool operator !=(Material? left, Material? right) => !Equals(left, right);
    }
}
