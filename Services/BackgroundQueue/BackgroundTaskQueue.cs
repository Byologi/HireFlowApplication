using System.Threading.Channels;

namespace HireFlow.Services.BackgroundQueue
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, Task>> _queue =
            Channel.CreateUnbounded<Func<IServiceProvider, Task>>();

        public void QueueTask(Func<IServiceProvider, Task> task)
        {
            _queue.Writer.TryWrite(task);
        }

        public async Task<Func<IServiceProvider, Task>> DequeueAsync(CancellationToken token)
        {
            return await _queue.Reader.ReadAsync(token);
        }
    }
}