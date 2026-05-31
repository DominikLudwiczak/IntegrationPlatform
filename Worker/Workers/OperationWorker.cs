using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Worker.Workers;

public class OperationWorker : BackgroundService
{
    private readonly IOperationQueue Queue;
    private readonly IServiceScopeFactory ScopeFactory;
    private readonly int DegreeOfParallelism;
    private readonly TimeSpan OperationTimeout;

    public OperationWorker(
        IOperationQueue queue,
        IServiceScopeFactory scopeFactory,
        int degreeOfParallelism = 4,
        TimeSpan? operationTimeout = null)
    {
        Queue = queue;
        ScopeFactory = scopeFactory;
        DegreeOfParallelism = degreeOfParallelism;
        OperationTimeout = operationTimeout ?? TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var semaphore = new SemaphoreSlim(DegreeOfParallelism);
        var activeTasks = new System.Collections.Concurrent.ConcurrentBag<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var operationId = await Queue.DequeueAsync(stoppingToken);
            if (operationId is null) continue;

            await semaphore.WaitAsync(stoppingToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    using var scope = ScopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOperationProcessor>();

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    cts.CancelAfter(OperationTimeout);

                    await processor.ProcessAsync(operationId.Value, cts.Token);
                }
                finally
                {
                    semaphore.Release();
                }
            }, stoppingToken);

            activeTasks.Add(task);
        }
        
        await Task.WhenAll(activeTasks);
    }
}