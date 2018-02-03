using System;
using System.Data.Entity;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace AspNetSkeleton.DataAccess.MySql
{
    public class MySqlDbConfiguration : DbConfiguration
    {
        public static string ProviderName => MySqlProviderInvariantName.ProviderName;
        public static Type SqlClientFactoryType => typeof(MySqlClientFactory);

        public MySqlDbConfiguration()
        {
            SetProviderFactory(ProviderName, new MySqlClientFactory());
            SetProviderServices(ProviderName, new MySqlProviderServices());
            SetMigrationSqlGenerator(ProviderName, () => new MySqlMigrationSqlGenerator());
            SetHistoryContext(ProviderName, (ec, ds) => new MySqlHistoryContext(ec, ds));
        }
    }
}
