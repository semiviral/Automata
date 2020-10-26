#region

using System.Collections.Generic;
using Automata.Engine.Numerics;

#endregion


namespace Automata.Engine.Collections
{
    public interface INodeCollection<T>
    {
        T Value { get; }
        bool IsUniform { get; }
        int Length { get; }

        T GetPoint(Vector3i point);
        T GetPoint(int x, int y, int z);
        void SetPoint(Vector3i point, T value);
        void SetPoint(int x, int y, int z, T value);
    }
}
