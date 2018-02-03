using System.Threading;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork
{
    public interface IShutDownTokenAccessor
    {
        CancellationToken ShutDownToken { get; }
    }
}
