using System;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.DataAccess
{
	public static partial class AsyncExtensions
	{
		#region FirstAsync<TSource>
		public static Task<TSource> FirstAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.FirstAsync(Unwrap(source), token);
		}
		#endregion

		#region FirstAsync<TSource, predicate>
		public static Task<TSource> FirstAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.FirstAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region FirstOrDefaultAsync<TSource>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.FirstOrDefaultAsync(Unwrap(source), token);
		}
		#endregion

		#region FirstOrDefaultAsync<TSource, predicate>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.FirstOrDefaultAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region SingleAsync<TSource>
		public static Task<TSource> SingleAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SingleAsync(Unwrap(source), token);
		}
		#endregion

		#region SingleAsync<TSource, predicate>
		public static Task<TSource> SingleAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SingleAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region SingleOrDefaultAsync<TSource>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SingleOrDefaultAsync(Unwrap(source), token);
		}
		#endregion

		#region SingleOrDefaultAsync<TSource, predicate>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SingleOrDefaultAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region ContainsAsync<TSource, item>
		public static Task<bool> ContainsAsync<TSource>(
			this IQueryable<TSource> source, TSource item,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.ContainsAsync(Unwrap(source), item, token);
		}
		#endregion

		#region AnyAsync<TSource>
		public static Task<bool> AnyAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AnyAsync(Unwrap(source), token);
		}
		#endregion

		#region AnyAsync<TSource, predicate>
		public static Task<bool> AnyAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AnyAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region AllAsync<TSource, predicate>
		public static Task<bool> AllAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AllAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region CountAsync<TSource>
		public static Task<int> CountAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.CountAsync(Unwrap(source), token);
		}
		#endregion

		#region CountAsync<TSource, predicate>
		public static Task<int> CountAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.CountAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region LongCountAsync<TSource>
		public static Task<long> LongCountAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.LongCountAsync(Unwrap(source), token);
		}
		#endregion

		#region LongCountAsync<TSource, predicate>
		public static Task<long> LongCountAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,bool>> predicate,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.LongCountAsync(Unwrap(source), predicate, token);
		}
		#endregion

		#region MinAsync<TSource>
		public static Task<TSource> MinAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.MinAsync(Unwrap(source), token);
		}
		#endregion

		#region MinAsync<TSource, selector>
		public static Task<TResult> MinAsync<TSource,TResult>(
			this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.MinAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region MaxAsync<TSource>
		public static Task<TSource> MaxAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.MaxAsync(Unwrap(source), token);
		}
		#endregion

		#region MaxAsync<TSource, selector>
		public static Task<TResult> MaxAsync<TSource,TResult>(
			this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.MaxAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<int>
		public static Task<int> SumAsync(
			this IQueryable<int> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<int?>
		public static Task<int?> SumAsync(
			this IQueryable<int?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<long>
		public static Task<long> SumAsync(
			this IQueryable<long> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<long?>
		public static Task<long?> SumAsync(
			this IQueryable<long?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<float>
		public static Task<float> SumAsync(
			this IQueryable<float> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<float?>
		public static Task<float?> SumAsync(
			this IQueryable<float?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<double>
		public static Task<double> SumAsync(
			this IQueryable<double> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<double?>
		public static Task<double?> SumAsync(
			this IQueryable<double?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<decimal>
		public static Task<decimal> SumAsync(
			this IQueryable<decimal> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<decimal?>
		public static Task<decimal?> SumAsync(
			this IQueryable<decimal?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), token);
		}
		#endregion

		#region SumAsync<int, selector>
		public static Task<int> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,int>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<int?, selector>
		public static Task<int?> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,int?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<long, selector>
		public static Task<long> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,long>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<long?, selector>
		public static Task<long?> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,long?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<float, selector>
		public static Task<float> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,float>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<float?, selector>
		public static Task<float?> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,float?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<double, selector>
		public static Task<double> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,double>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<double?, selector>
		public static Task<double?> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,double?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<decimal, selector>
		public static Task<decimal> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,decimal>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region SumAsync<decimal?, selector>
		public static Task<decimal?> SumAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,decimal?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.SumAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<int>
		public static Task<double> AverageAsync(
			this IQueryable<int> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<int?>
		public static Task<double?> AverageAsync(
			this IQueryable<int?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<long>
		public static Task<double> AverageAsync(
			this IQueryable<long> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<long?>
		public static Task<double?> AverageAsync(
			this IQueryable<long?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<float>
		public static Task<float> AverageAsync(
			this IQueryable<float> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<float?>
		public static Task<float?> AverageAsync(
			this IQueryable<float?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<double>
		public static Task<double> AverageAsync(
			this IQueryable<double> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<double?>
		public static Task<double?> AverageAsync(
			this IQueryable<double?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<decimal>
		public static Task<decimal> AverageAsync(
			this IQueryable<decimal> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<decimal?>
		public static Task<decimal?> AverageAsync(
			this IQueryable<decimal?> source,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), token);
		}
		#endregion

		#region AverageAsync<int, selector>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,int>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<int?, selector>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,int?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<long, selector>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,long>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<long?, selector>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,long?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<float, selector>
		public static Task<float> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,float>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<float?, selector>
		public static Task<float?> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,float?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<double, selector>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,double>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<double?, selector>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,double?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<decimal, selector>
		public static Task<decimal> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,decimal>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

		#region AverageAsync<decimal?, selector>
		public static Task<decimal?> AverageAsync<TSource>(
			this IQueryable<TSource> source, Expression<Func<TSource,decimal?>> selector,
			CancellationToken token = default(CancellationToken))
		{
			return LinqToDB.AsyncExtensions.AverageAsync(Unwrap(source), selector, token);
		}
		#endregion

	}
}