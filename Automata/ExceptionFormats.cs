namespace Automata
{
    public static class ExceptionFormats
    {
        public static string ArgumentNullException => "Argument cannot be null.";
        public static string ComponentInstanceExistsException => "Entity already contains component instance.";
        public static string EntityManagerAsynchronousAccessException => "Cannot modify entity collection asynchronously.";
    }
}
