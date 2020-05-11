#region

using Automata.Core.Systems;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Core.Components
{
    /// <summary>
    ///     Used to expose an <see cref="IView" /> to the <see cref="SystemManager" />, usually to run DoUpdate() and
    ///     DoRender().
    /// </summary>
    public class WindowIViewProvider : IComponent
    {
        public IView View { get; }

        public WindowIViewProvider(IView view) => View = view;
    }
}
