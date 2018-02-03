using System;
using AspNetSkeleton.Service.Contract;

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
