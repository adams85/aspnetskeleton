namespace AspNetSkeleton.Common.Infrastructure
{
    public interface IKeyedProvider<out T>
    {
        T ProvideFor(object key);
    }

    public static class KeyedProvider
    {
        public static readonly object Default = new object();
    }
}
