#region

using System;
using System.Runtime.CompilerServices;

// ReSharper disable TypeParameterCanBeVariant

#endregion


namespace Automata.Game.Blocks
{
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

        private static readonly Func<Direction, string> _DefaultUVsRule;

        private readonly Func<Direction, string> _UVsRule;

        static Block() => _DefaultUVsRule = _ => string.Empty;

        public Block(ushort id, string blockName, Func<Direction, string>? uvsRule, params Attribute[] properties)
        {
            ID = id;
            BlockName = blockName;

            foreach (Attribute property in properties) Attributes |= property;

            _UVsRule = uvsRule ?? _DefaultUVsRule;
        }

        public ushort ID { get; }
        public string BlockName { get; }
        public Attribute Attributes { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(Attribute flag) => (Attributes & flag) == flag;

        public bool GetTextureName(Direction direction, out string spriteName)
        {
            spriteName = _UVsRule(direction);
            return true;
        }
    }
}
