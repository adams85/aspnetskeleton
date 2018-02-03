using Karambolo.Common;
using System.Collections.Generic;

namespace AspNetSkeleton.DataAccess
{
    public class DataContext : DbContext<DataContext>
    {
        public DataContext(IDbConfigurationProvider configurationProvider) : base(configurationProvider) { }

        public override IReadOnlyList<string> MigrationHistory { get; } = ArrayUtils.FromElements
        (
            "InitialCreate"
        );
    }
}