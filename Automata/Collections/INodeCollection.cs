#region

using System.Collections.Generic;
using Automata.Numerics;

#endregion

namespace Automata.Collections
{
    public interface INodeCollection<T>
    {
        T Value { get; }
        bool IsUniform { get; }
        int Length { get; }

        T GetPoint(Vector3i point);
        void SetPoint(Vector3i point, T value);

        IEnumerable<T> GetAllData();
        void CopyTo(T[] destinationArray);
    }
}
