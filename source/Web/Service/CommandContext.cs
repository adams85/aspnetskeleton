using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service
{
    public interface ICommandContext
    {
        IReadWriteDataAccessScope CreateDataAccessScope();
    }

    public class CommandContext : ICommandContext
    {
        class DataAccessScope : IReadWriteDataAccessScope
        {
            class Root : DataAccessScope
            {
                readonly IDbContext _context;

                public Root(DataAccessScope parent, IKeyedProvider<IDbContext> contextProvider) : base(parent)
                {
                    _context = contextProvider.ProvideFor(typeof(DataContext));
                }

                public override IReadWriteDbContext Context => _context;

                public override int SaveChanges()
                {
                    if (_owner._changesHasSaved)
                        throw new InvalidOperationException();

                    _owner._changesHasSaved = true;

                    return _context.SaveChanges();
                }

                public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
                {
                    if (_owner._changesHasSaved)
                        throw new InvalidOperationException();

                    _owner._changesHasSaved = true;

                    return _context.SaveChangesAsync(cancellationToken);
                }

                public override DataAccessScope CreateChildScope(IKeyedProvider<IDbContext> contextProvider)
                {
                    return new Nested(this);
                }                

                protected override void DisposeCore()
                {
                    _context?.Dispose();
                }
            }

            class Nested : DataAccessScope
            {
                public Nested(DataAccessScope parent) : base(parent) { }

                public override IReadWriteDbContext Context => _parent.Context;

                public override int SaveChanges()
                {
                    return -1;
                }

                public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
                {
                    return Task.FromResult(-1);
                }

                public override DataAccessScope CreateChildScope(IKeyedProvider<IDbContext> contextProvider)
                {
                    return new Nested(this);
                }
            }

            readonly CommandContext _owner;
            readonly DataAccessScope _parent;
            bool _isDisposed;

            DataAccessScope(DataAccessScope parent)
            {
                _owner = parent._owner;
                _parent = parent;
            }

            public DataAccessScope(CommandContext owner)
            {
                _owner = owner;
            }

            public virtual IReadWriteDbContext Context => throw new InvalidOperationException();

            protected virtual void DisposeCore() { }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    if (_owner._currentDataAccessScope != this)
                        throw new InvalidOperationException();

                    _owner._currentDataAccessScope = _parent;

                    DisposeCore();
                    _isDisposed = true;
                }
            }

            public virtual int SaveChanges()
            {
                throw new InvalidOperationException();
            }

            public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            {
                throw new InvalidOperationException();
            }

            public virtual DataAccessScope CreateChildScope(IKeyedProvider<IDbContext> contextProvider)
            {
                return new Root(this, contextProvider);
            }
        }

        readonly IKeyedProvider<IDbContext> _dbContextProvider;
        DataAccessScope _currentDataAccessScope;
        bool _changesHasSaved;

        public CommandContext(IKeyedProvider<IDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
            _currentDataAccessScope = new DataAccessScope(this);
        }

        public IReadWriteDataAccessScope CreateDataAccessScope()
        {
            return _currentDataAccessScope = _currentDataAccessScope.CreateChildScope(_dbContextProvider);
        }
    }
}
