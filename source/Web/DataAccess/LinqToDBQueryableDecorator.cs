using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AspNetSkeleton.DataAccess.Utils;
using Karambolo.Common;
using LinqToDB;

namespace AspNetSkeleton.DataAccess
{
    // WORKAROUND: for issue described at https://github.com/linq2db/linq2db/issues/819
    // Hopefully, the next major version of linq2db will make this unnecessary.

    class LinqToDBQueryableDecorator : IQueryableDecorator
    {
        class Visitor : ExpressionVisitor
        {
            static MethodInfo asSqlMethodDefinition = Lambda.Method(() => Sql.AsSql<object>(null)).GetGenericMethodDefinition();
            static MethodInfo evaluateMethodDefinition = Lambda.Method(() => SqlHelper.Evaluate<object>(null)).GetGenericMethodDefinition();

            public static readonly Visitor Instance = new Visitor();

            Visitor() { }

            static bool IsConstantRootedMemberAccessPath(MemberExpression node)
            {
                while (true)
                {
                    var expression = node.Expression;
                    var nodeType = expression?.NodeType;

                    if (nodeType != ExpressionType.MemberAccess)
                        return nodeType == ExpressionType.Constant;

                    node = (MemberExpression)expression;
                }
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                MethodInfo methodDefinition;
                if (node.Method.IsGenericMethod &&
                    ((methodDefinition = node.Method.GetGenericMethodDefinition()) == asSqlMethodDefinition || methodDefinition == evaluateMethodDefinition))
                    return node;

                if (node.Object?.NodeType == ExpressionType.MemberAccess &&
                    IsConstantRootedMemberAccessPath((MemberExpression)node.Object))
                {
                    var asSqlMethod = asSqlMethodDefinition.MakeGenericMethod(node.Type);
                    return Expression.Call(Expression.Call(asSqlMethod, node.Object), node.Method);
                }

                return base.VisitMethodCall(node);
            }
        }

        class ProviderDecorator : IQueryProvider
        {
            readonly IQueryProvider _target;

            public ProviderDecorator(IQueryProvider target)
            {
                _target = target;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                expression = Visitor.Instance.Visit(expression);
                return new LinqToDBQueryableDecorator(_target.CreateQuery(expression));
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                expression = Visitor.Instance.Visit(expression);
                return new LinqToDBQueryableDecorator<TElement>(_target.CreateQuery<TElement>(expression));
            }

            public object Execute(Expression expression)
            {
                return _target.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _target.Execute<TResult>(expression);
            }
        }

        public LinqToDBQueryableDecorator(IQueryable target)
        {
            Target = target;
        }

        public Expression Expression => Target.Expression;

        public Type ElementType => Target.ElementType;

        public IQueryProvider Provider => new ProviderDecorator(Target.Provider);

        public IQueryable Target { get; }

        public IEnumerator GetEnumerator()
        {
            return Target.GetEnumerator();
        }
    }

    class LinqToDBQueryableDecorator<T> : LinqToDBQueryableDecorator, IQueryableDecorator<T>
    {
        public LinqToDBQueryableDecorator(IQueryable<T> target) : base(target) { }

        public new IQueryable<T> Target => (IQueryable<T>)base.Target;

        public new IEnumerator<T> GetEnumerator()
        {
            return Target.GetEnumerator();
        }
    }
}
