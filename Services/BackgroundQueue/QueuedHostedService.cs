using HireFlow.Infrastructure.Data;

namespace HireFlow.Services.BackgroundQueue
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceProvider _provider;

        public QueuedHostedService(IBackgroundTaskQueue queue, IServiceProvider provider)
        {
            _queue = queue;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var task = await _queue.DequeueAsync(stoppingToken);

                using var scope = _provider.CreateScope();
                await task(scope.ServiceProvider);
            }
        }
    }
}