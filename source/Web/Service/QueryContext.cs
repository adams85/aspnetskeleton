using System;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service
{
    public interface IQueryContext
    {
        IReadOnlyDataAccessScope CreateDataAccessScope();
    }

    public class QueryContext : IQueryContext
    {
        class DataAccessScope : IReadOnlyDataAccessScope
        {
            class Root : DataAccessScope
            {
                readonly IReadOnlyDbContext _context;

                public Root(DataAccessScope parent, IKeyedProvider<IReadOnlyDbContext> contextProvider) : base(parent)
                {
                    _context = contextProvider.ProvideFor(typeof(DataContext));
                }

                public override IReadOnlyDbContext Context => _context;

                public override DataAccessScope CreateChildScope(IKeyedProvider<IReadOnlyDbContext> contextProvider)
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

                public override IReadOnlyDbContext Context => _parent.Context;

                public override DataAccessScope CreateChildScope(IKeyedProvider<IReadOnlyDbContext> contextProvider)
                {
                    return new Nested(this);
                }
            }

            readonly QueryContext _owner;
            readonly DataAccessScope _parent;
            bool _isDisposed;

            DataAccessScope(DataAccessScope parent)
            {
                _owner = parent._owner;
                _parent = parent;
            }

            public DataAccessScope(QueryContext owner)
            {
                _owner = owner;
            }

            protected DataAccessScope Parent => _parent;

            public virtual IReadOnlyDbContext Context => throw new InvalidOperationException();

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

            public virtual DataAccessScope CreateChildScope(IKeyedProvider<IReadOnlyDbContext> contextProvider)
            {
                return new Root(this, contextProvider);
            }
        }

        readonly IKeyedProvider<IReadOnlyDbContext> _dbContextProvider;
        DataAccessScope _currentDataAccessScope;

        public QueryContext(IKeyedProvider<IReadOnlyDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
            _currentDataAccessScope = new DataAccessScope(this);
        }

        public IReadOnlyDataAccessScope CreateDataAccessScope()
        {
            return _currentDataAccessScope =_currentDataAccessScope.CreateChildScope(_dbContextProvider);
        }
    }
}
