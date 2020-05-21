#region

using System;
using Serilog;

#endregion

namespace Automata
{
    public static class Singleton
    {
        public static void CreateSingleton<T>() where T : Singleton<T>, new() => new T();
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
                    throw new NullReferenceException($"'{typeof(T)}' has not been instantiated.");
                }
                else
                {
                    return _Instance;
                }
            }
        }

        public static void Validate()
        {
            if (!(Instance is object))
            {
                throw new InvalidOperationException($"Singleton '{typeof(T)}' has not been instantiated.");
            }
        }

        public static bool TryValidate() => Instance is object;

        protected string _LogFormat { get; } = $"({typeof(T).Name}) {{0}}";

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

                Log.Information($"Singleton '{typeof(T)}' has been instantiated.");
            }
        }
    }
}
