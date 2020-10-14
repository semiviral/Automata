#region

using Automata.Systems;

#endregion

namespace Automata.Diagnostics
{
    public class DiagnosticsSystem : ComponentSystem
    {
        public DiagnosticsSystem() => HandledComponents = new ComponentTypes();
    }
}
