#region

using System;

// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace Automata.Rendering.OpenGL
{
    public class UniformNotFoundException : Exception
    {
        public string Name { get; }

        public UniformNotFoundException(string name) : base($"Uniform '{name}' not found in shader.") => Name = name;
    }
}
