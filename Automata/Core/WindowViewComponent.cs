#region

using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Core
{
    /// <summary>
    ///     Used to expose an <see cref="IView"/> to the <see cref="SystemManager"/>, usually to run DoUpdate() and DoRender().
    /// </summary>
    public class WindowViewComponent : IComponent
    {
        public IView View { get; }

        public WindowViewComponent(IView view) => View = view;
    }
}
