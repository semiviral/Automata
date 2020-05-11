#region

using System;
using Automata.Core;
using Automata.Core.Components;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    /// <summary>
    ///     Used to expose an <see cref="IInputContext"/>.
    /// </summary>
    public class InputContextProvider : IComponent
    {
        public Guid InputContextID { get; } = Guid.NewGuid();
        public IInputContext InputContext { get; }

        public InputContextProvider(IInputContext inputContext)
        {
            InputContext = inputContext ?? throw new NullReferenceException(nameof(inputContext));
        }
    }
}
