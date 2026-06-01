using Microsoft.Extensions.DependencyInjection;
using Worker.Workers;

namespace Worker.Tests.Workers;

[TestClass]
public class OperationWorkerTests
{
    private IOperationQueue _queue = null!;
    private IServiceScopeFactory _scopeFactory = null!;
    private IOperationProcessor _processor = null!;
    private OperationWorker _worker = null!;

    [TestInitialize]
    public void Setup()
    {
        _queue = Substitute.For<IOperationQueue>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _processor = Substitute.For<IOperationProcessor>();
        _worker = new OperationWorker(_queue, _scopeFactory);

        // GetRequiredService<T> is an extension method over IServiceProvider.GetService(Type)
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IOperationProcessor)).Returns(_processor);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        _scopeFactory.CreateScope().Returns(scope);
    }

    [TestMethod]
    [Timeout(3000)]
    public async Task ExecuteAsync_WhenQueueReturnsNull_DoesNotCallProcessor()
    {
        // Signal when the loop has iterated past the null value (second dequeue call means
        // the null was processed and the loop continued).
        var loopContinued = new TaskCompletionSource();
        int callCount = 0;

        _queue.DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var token = ci.Arg<CancellationToken>();
                if (Interlocked.Increment(ref callCount) == 1)
                    return Task.FromResult<Guid?>(null);

                loopContinued.TrySetResult();
                var tcs = new TaskCompletionSource<Guid?>();
                token.Register(() => tcs.TrySetCanceled());
                return tcs.Task;
            });

        await _worker.StartAsync(CancellationToken.None);
        await loopContinued.Task;
        await _worker.StopAsync(CancellationToken.None);

        await _processor.DidNotReceive().ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [Timeout(3000)]
    public async Task ExecuteAsync_WhenQueueReturnsOperationId_CallsProcessorWithThatId()
    {
        var operationId = Guid.NewGuid();
        var processed = new TaskCompletionSource();
        int callCount = 0;

        _queue.DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var token = ci.Arg<CancellationToken>();
                if (Interlocked.Increment(ref callCount) == 1)
                    return Task.FromResult<Guid?>(operationId);

                var tcs = new TaskCompletionSource<Guid?>();
                token.Register(() => tcs.TrySetCanceled());
                return tcs.Task;
            });

        _processor.ProcessAsync(operationId, Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                processed.TrySetResult();
                return Task.FromResult(0);
            });

        await _worker.StartAsync(CancellationToken.None);
        await processed.Task;
        await _worker.StopAsync(CancellationToken.None);

        await _processor.Received(1).ProcessAsync(operationId, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [Timeout(3000)]
    public async Task ExecuteAsync_WhenMultipleIdsDequeued_CallsProcessorForEach()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var ids = new Queue<Guid?>(new[] { (Guid?)id1, id2 });
        var bothProcessed = new TaskCompletionSource();
        int processCount = 0;

        _queue.DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var token = ci.Arg<CancellationToken>();
                if (ids.TryDequeue(out var next))
                    return Task.FromResult(next);

                var tcs = new TaskCompletionSource<Guid?>();
                token.Register(() => tcs.TrySetCanceled());
                return tcs.Task;
            });

        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                if (Interlocked.Increment(ref processCount) == 2)
                    bothProcessed.TrySetResult();
                return Task.FromResult(0);
            });

        await _worker.StartAsync(CancellationToken.None);
        await bothProcessed.Task;
        await _worker.StopAsync(CancellationToken.None);

        await _processor.Received(1).ProcessAsync(id1, Arg.Any<CancellationToken>());
        await _processor.Received(1).ProcessAsync(id2, Arg.Any<CancellationToken>());
    }
}
