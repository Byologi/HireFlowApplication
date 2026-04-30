using HireFlow.Services.BackgroundQueue;

public class FakeBackgroundQueue : IBackgroundTaskQueue
{
    public void QueueTask(Func<IServiceProvider, Task> task)
    {
        // do nothing (we don't test background jobs here)
    }

    public Task<Func<IServiceProvider, Task>> DequeueAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}