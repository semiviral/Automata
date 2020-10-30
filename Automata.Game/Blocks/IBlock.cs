namespace Automata.Game.Blocks
{
    public interface IBlock
    {
        ushort ID { get; }
        string BlockName { get; }
        Block.Attribute Attributes { get; }

        public bool HasAttribute(Block.Attribute attribute);
        bool GetTextureName(Direction direction, out string spriteName);
    }
}
