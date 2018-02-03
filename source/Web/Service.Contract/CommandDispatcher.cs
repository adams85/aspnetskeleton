using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Contract
{
    public interface ICommandDispatcher
    {
        Task DispatchAsync(ICommand command, CancellationToken cancellationToken);
    }
}
