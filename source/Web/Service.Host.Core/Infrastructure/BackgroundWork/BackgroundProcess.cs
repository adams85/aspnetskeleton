using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork
{
    public interface IBackgroundProcess
    {
        Task ExecuteAsync();
    }
}