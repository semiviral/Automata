#region

using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Core
{
    public class WindowViewComponent : IComponent
    {
        public IView View { get; }

        public WindowViewComponent(IView view) => View = view;
    }
}
