using System;

namespace Automata.Core
{
    public class Component : IComponent
    {
        public static IComponent Default { get; } = new Component();
    }
}
