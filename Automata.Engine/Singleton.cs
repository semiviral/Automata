#region

#endregion


namespace Automata.Engine
{
    public class Singleton<T> where T : class, new()
    {
        public static T Instance { get; } = new T();

        /// <summary>
        ///     Use this function to force the lazy-initialization of the singleton.
        /// </summary>
        public void LazyInitialize() { }

        protected string _LogFormat { get; } = string.Format(FormatHelper.DEFAULT_LOGGING, typeof(T), "{0}");
    }
}
