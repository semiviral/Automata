namespace Automata.Engine
{
    public class Ring
    {
        private readonly nuint _Max;

        public nuint Current { get; private set; }

        public Ring(nuint max) => _Max = max;

        public void Increment() => Current = NextRing();
        public nuint NextRing() => (Current + 1u) % _Max;
    }
}
