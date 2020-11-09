namespace Automata.Engine
{
    public class Singleton<T> where T : class, new()
    {
        public static T Instance { get; private set; } = new T();

        protected string _LogFormat { get; } = string.Format(FormatHelper.DEFAULT_LOGGING, typeof(T), "{0}");

        /// <summary>
        ///     Use this function to force the lazy-initialization of the singleton.
        /// </summary>
        public void LazyInitialize() { }

        protected static void AssignInstance(T instance) => Instance = instance;
    }
}
