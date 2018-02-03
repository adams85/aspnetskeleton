using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Contract
{
    public interface IQueryDispatcher
    {
        Task<object> DispatchAsync(IQuery query, CancellationToken cancellationToken);
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    }
}
