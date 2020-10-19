namespace Automata.Engine.Components
{
    public class Scale : IComponentChangeable
    {
        public const float DEFAULT = 1f;

        private float _Value;

        public float Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public Scale() => Value = DEFAULT;

        public bool Changed { get; set; }
    }
}
