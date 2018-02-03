using LinqToDB;

namespace AspNetSkeleton.DataAccess.Utils
{
    public static class SqlHelper
    {
        [Sql.Expression("{0}", new[] { 0 }, PreferServerSide = false, ServerSideOnly = false)]
        public static T Evaluate<T>(T value)
        {
            return value;
        }
    }
}
