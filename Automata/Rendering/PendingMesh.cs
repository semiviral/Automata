#region

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Automata.Core.Components;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public class PendingMesh<T> : IComponent where T : unmanaged
    {
        public IEnumerable<T> Vertexes { get; set; } = Enumerable.Empty<T>();
        public IEnumerable<uint> Indexes { get; set; } = Enumerable.Empty<uint>();
    }
}
