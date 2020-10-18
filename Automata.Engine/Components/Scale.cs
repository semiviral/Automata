namespace Automata.Engine.Components
{
    public class Scale : IComponentChangeable
    {
        private float _Value = 1f;

        public float Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public bool Changed { get; set; }
    }
}
