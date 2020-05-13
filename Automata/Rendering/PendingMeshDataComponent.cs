#region

using System.Numerics;
using Automata.Core.Components;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public class PendingMeshDataComponent : IComponent
    {
        public Vector3[] Vertices { get; set; } = new Vector3[0];
        public uint[] Indices { get; set; } = new uint[0];
    }
}
