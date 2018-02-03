using System;
using System.Data.Entity;

namespace AspNetSkeleton.DataAccess
{
    public sealed class CheckIfDatabaseCompatible<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Database.Exists())
            {
                if (!context.Database.CompatibleWithModel(throwIfNoMetadata: false))
                    throw new InvalidOperationException($"The model backing the {context.GetType().Name} context has changed since the database was created");
            }
            else
                throw new InvalidOperationException($"The database of the model backing the {context.GetType().Name} context does not exist.");
        }
    }
}