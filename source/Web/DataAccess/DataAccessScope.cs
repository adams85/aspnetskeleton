using System;

namespace AspNetSkeleton.DataAccess
{
    public interface IReadOnlyDataAccessScope : IDisposable
    {
        IReadOnlyDbContext Context { get; }
    }

    public interface IReadWriteDataAccessScope : IUnitOfWork, IDisposable
    {
        IReadWriteDbContext Context { get; }
    }
}
