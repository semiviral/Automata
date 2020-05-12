#region

using System;

#endregion

namespace Automata
{
    public static class Singleton
    {
        public static void InstantiateSingleton<T>() where T : Singleton<T>, new() => new T();
    }

    public class Singleton<T>
    {
        private static Singleton<T>? _SingletonInstance;
        private static T _Instance = default!;

        public static T Instance
        {
            get
            {
                if (!(_Instance is object))
                {
                    throw new NullReferenceException("Singleton has not been instantiated.");
                }
                else
                {
                    return _Instance;
                }
            }
        }

        protected void AssignSingletonInstance(T instance)
        {
            if ((_SingletonInstance != default) && (_SingletonInstance != this))
            {
                throw new ArgumentException($"Singleton for type {typeof(T)} already exists.", nameof(instance));
            }
            else
            {
                _SingletonInstance = this;
                _Instance = instance;
            }
        }
    }
}
