#region

using System;
using Serilog;

#endregion


namespace Automata.Engine
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
                if (TryValidate()) return _Instance;
                else throw new NullReferenceException($"'{typeof(T)}' has not been instantiated.");
            }
        }

        public static void Validate()
        {
            if (Instance is null) throw new InvalidOperationException($"Singleton '{typeof(T)}' has not been instantiated.");
        }

        public static bool TryValidate() => _Instance is not null;

        protected string _LogFormat { get; } = string.Format(FormatHelper.DEFAULT_LOGGING, typeof(T), "{0}");

        protected void AssignSingletonInstance(T instance)
        {
            if ((_SingletonInstance != default) && (_SingletonInstance != this))
                throw new ArgumentException($"Singleton for type {typeof(T)} already exists.", nameof(instance));
            else
            {
                _SingletonInstance = this;
                _Instance = instance;

                Log.Information($"Singleton '{typeof(T)}' has been instantiated.");
            }
        }
    }
}
