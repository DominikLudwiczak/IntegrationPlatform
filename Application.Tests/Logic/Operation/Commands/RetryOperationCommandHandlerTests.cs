using Application.Logic.Operation.Commands;
using MockQueryable.NSubstitute;

namespace Application.Tests.Logic.Operation.Commands;

[TestClass]
public class RetryOperationCommandHandlerTests
{
    private IApplicationDbContext _context = null!;
    private IOperationQueue _queue = null!;
    private RetryOperationCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _queue = Substitute.For<IOperationQueue>();
        _handler = new RetryOperationCommandHandler(_context, _queue);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    private void SetupDbSet(params OperationRecord[] records)
    {
        var mockDbSet = records.AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
    }

    private static OperationRecord MakeFailedOperation(Guid id, int retryCount = 0, int maxRetries = 3) =>
        new() { Id = id, Status = OperationStatusEnum.Failed, MaxRetries = maxRetries, RetryCount = retryCount };

    [TestMethod]
    public async Task Handle_OperationNotFound_ReturnsFailureCode1()
    {
        SetupDbSet();
        var result = await _handler.Handle(new RetryOperationCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-1, result.Code);
    }

    [TestMethod]
    public async Task Handle_OperationIsPending_ReturnsFailureCode2()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Pending, MaxRetries = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-2, result.Code);
    }

    [TestMethod]
    public async Task Handle_OperationIsRunning_ReturnsFailureCode2()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Running, MaxRetries = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-2, result.Code);
    }

    [TestMethod]
    public async Task Handle_OperationIsCompleted_ReturnsFailureCode2()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Completed, MaxRetries = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-2, result.Code);
    }

    [TestMethod]
    public async Task Handle_RetryCountAtMaxRetries_ReturnsFailureCode3()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Failed, MaxRetries = 3, RetryCount = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-3, result.Code);
    }

    [TestMethod]
    public async Task Handle_ValidRetry_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakeFailedOperation(id));

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public async Task Handle_ValidRetry_IncrementsRetryCount()
    {
        var id = Guid.NewGuid();
        var operation = MakeFailedOperation(id, retryCount: 1, maxRetries: 3);
        SetupDbSet(operation);

        await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.AreEqual(2, operation.RetryCount);
    }

    [TestMethod]
    public async Task Handle_ValidRetry_SetsPendingStatus()
    {
        var id = Guid.NewGuid();
        var operation = MakeFailedOperation(id);
        SetupDbSet(operation);

        await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.AreEqual(OperationStatusEnum.Pending, operation.Status);
    }

    [TestMethod]
    public async Task Handle_ValidRetry_ClearsIsTimedOut()
    {
        var id = Guid.NewGuid();
        var operation = new OperationRecord { Id = id, Status = OperationStatusEnum.TimedOut, IsTimedOut = true, MaxRetries = 3 };
        SetupDbSet(operation);

        await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsFalse(operation.IsTimedOut);
    }

    [TestMethod]
    public async Task Handle_ValidRetry_SavesAndEnqueues()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakeFailedOperation(id));

        await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _queue.Received(1).EnqueueAsync(id, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Handle_TimedOutOperation_CanBeRetried()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.TimedOut, IsTimedOut = true, MaxRetries = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public async Task Handle_CancelledOperation_CanBeRetried()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Cancelled, MaxRetries = 3 });

        var result = await _handler.Handle(new RetryOperationCommand { Id = id }, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
    }
}
