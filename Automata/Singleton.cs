#region

using System;

#endregion

namespace Automata
{
    public class Singleton<T>
    {
        private static Singleton<T> _SingletonInstance;

        public static T Instance { get; private set; }

        protected virtual void AssignSingletonInstance(T instance)
        {
            if ((_SingletonInstance != default) && (_SingletonInstance != this))
            {
                throw new ArgumentException($"Singleton for type {typeof(T)} already exists.", nameof(instance));
            }
            else
            {
                _SingletonInstance = this;
                Instance = instance;
            }
        }
    }
}
