#region

using System;

// ReSharper disable TypeParameterCanBeVariant

#endregion


namespace Automata.Game.Blocks
{
    public class BlockDefinition : IBlockDefinition
    {
        [Flags]
        public enum Property
        {
            Transparent = 1,
            Collideable = 2,
            Destroyable = 4,
            Collectible = 8,
            LightSource = 16
        }

        private static readonly Func<Direction, string> _DefaultUVsRule;

        private readonly Func<Direction, string> _UVsRule;

        public ushort Id { get; }

        static BlockDefinition() { _DefaultUVsRule = direction => string.Empty; }

        public BlockDefinition(ushort id, string blockName, Func<Direction, string> uvsRule, params Property[] properties)
        {
            Id = id;
            BlockName = blockName;

            foreach (Property property in properties) Properties |= property;

            _UVsRule = uvsRule ?? _DefaultUVsRule;
        }

        public string BlockName { get; }
        public Property Properties { get; }

        public bool Transparent => (Properties & Property.Transparent) == Property.Transparent;
        public bool Collideable => (Properties & Property.Collideable) == Property.Collideable;
        public bool Destroyable => (Properties & Property.Destroyable) == Property.Destroyable;
        public bool Collectible => (Properties & Property.Collectible) == Property.Collectible;
        public bool LightSource => (Properties & Property.LightSource) == Property.LightSource;

        public virtual bool GetUVs(Direction direction, out string spriteName)
        {
            spriteName = _UVsRule(direction);
            return true;
        }

        public bool HasProperty(Property flag) => (Properties & flag) == flag;
    }
}
