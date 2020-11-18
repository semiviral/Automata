using System;
using System.Runtime.CompilerServices;

// ReSharper disable TypeParameterCanBeVariant

namespace Automata.Game.Blocks
{
    public interface IBlockDefinition
    {
        [Flags]
        public enum Attribute
        {
            Transparent = 1 << 0,
            Collideable = 1 << 1,
            Destructible = 1 << 2,
            Collectible = 1 << 3
        }

        ushort ID { get; }
        string BlockName { get; }
        public int MeshingStrategyIndex { get; }
        Attribute Attributes { get; }

        public bool HasAttribute(Attribute attribute);
    }

    public sealed class BlockDefinition : IBlockDefinition
    {
        public ushort ID { get; }
        public string BlockName { get; }
        public int MeshingStrategyIndex { get; }
        public IBlockDefinition.Attribute Attributes { get; }

        public BlockDefinition(ushort id, string blockName, int meshingStrategyIndex, params IBlockDefinition.Attribute[] properties)
        {
            ID = id;
            BlockName = blockName;
            MeshingStrategyIndex = meshingStrategyIndex;

            foreach (IBlockDefinition.Attribute property in properties)
            {
                Attributes |= property;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(IBlockDefinition.Attribute flag) => (Attributes & flag) == flag;
    }
}
