using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common;
using LinqToDB.Common;
using MySql.Data.MySqlClient;

namespace AspNetSkeleton.DataAccess.MySql
{
    public class MySqlDbManager : DbManager
    {
        readonly string _databaseName;
        readonly string _connectionStringWithoutDb;

        public MySqlDbManager(DbContext context, IClock clock)
            : base(context, clock)
        {
            if (context.ProviderName != DataAccessContants.MySqlProviderName)
                throw new ArgumentException("Mismatching data provider.", nameof(context));

            var builder = new DbConnectionStringBuilder(useOdbcRules: false);
            builder.ConnectionString = Context.ConnectionString;
            _databaseName = builder["database"] as string;
            builder.Remove("database");
            _connectionStringWithoutDb = builder.ConnectionString;
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            using (var connection = Context.DataProvider.CreateConnection(_connectionStringWithoutDb))
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = "SHOW DATABASES LIKE @databaseName";

                var param = command.CreateParameter();
                Context.DataProvider.SetParameter(param, "databaseName", new DbDataType(typeof(string)), _databaseName);
                command.Parameters.Add(param);

                return (await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) != null;
            }
        }

        public override async Task CreateAsync(CancellationToken cancellationToken)
        {
            using (var connection = Context.DataProvider.CreateConnection(_connectionStringWithoutDb))
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = $"CREATE DATABASE `{_databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task DropAsync(CancellationToken cancellationToken)
        {
            using (var connection = Context.DataProvider.CreateConnection(_connectionStringWithoutDb))
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = $"DROP DATABASE `{_databaseName}`";

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        protected override async Task<bool> HasMigrationInfoAsync(CancellationToken cancellationToken)
        {
            using (var command = (MySqlCommand)Context.CreateCommand())
            {
                command.CommandText = "SHOW TABLES LIKE @tableName";

                var param = command.CreateParameter();
                Context.DataProvider.SetParameter(param, "tableName", new DbDataType(typeof(string)), migrationInfoTableName);
                command.Parameters.Add(param);

                return (await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) != null;
            }
        }

        protected override Task ExecuteScriptAsync(string content, CancellationToken cancellationToken)
        {
            var scriptExecutor = new MySqlScript((MySqlConnection)Context.Connection, content);
            return scriptExecutor.ExecuteAsync(cancellationToken);
        }
    }
}
