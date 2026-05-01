using System.Threading.Channels;

namespace HireFlow.Services.BackgroundQueue
{
    public interface IBackgroundTaskQueue
    {
        void QueueTask(Func<IServiceProvider, Task> task);
        Task<Func<IServiceProvider, Task>> DequeueAsync(CancellationToken token);
    }
}