#region

using System;
using System.Runtime.CompilerServices;

// ReSharper disable TypeParameterCanBeVariant

#endregion


namespace Automata.Game.Blocks
{
    public interface IBlock
    {
        ushort ID { get; }
        string BlockName { get; }
        public int MeshingStrategyIndex { get; }
        Block.Attribute Attributes { get; }

        public bool HasAttribute(Block.Attribute attribute);
    }

    public sealed class Block : IBlock
    {
        [Flags]
        public enum Attribute
        {
            Transparent = 1 << 0,
            Collideable = 1 << 1,
            Destructible = 1 << 2,
            Collectible = 1 << 3
        }

        public ushort ID { get; }
        public string BlockName { get; }
        public int MeshingStrategyIndex { get; }
        public Attribute Attributes { get; }

        public Block(ushort id, string blockName, int meshingStrategyIndex, params Attribute[] properties)
        {
            ID = id;
            BlockName = blockName;
            MeshingStrategyIndex = meshingStrategyIndex;

            foreach (Attribute property in properties) Attributes |= property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(Attribute flag) => (Attributes & flag) == flag;
    }
}
