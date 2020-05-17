#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public class PendingMesh<TDataType> : IComponent where TDataType : unmanaged
    {
        public IEnumerable<TDataType> Vertexes { get; set; } = Enumerable.Empty<TDataType>();
        public IEnumerable<uint> Indexes { get; set; } = Enumerable.Empty<uint>();
    }
}
