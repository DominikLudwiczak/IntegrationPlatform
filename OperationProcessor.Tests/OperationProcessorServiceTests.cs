using MockQueryable.NSubstitute;

namespace OperationProcessor.Tests;

[TestClass]
public class OperationProcessorServiceTests
{
    private IApplicationDbContext _context = null!;
    private OperationHandler _handler = null!;
    private OperationProcessorService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = Substitute.For<OperationHandler>(Substitute.For<IErrorSimulator>());
        _handler.OperationType = OperationTypeEnum.DataSync;

        _service = new OperationProcessorService(_context, new[] { _handler });

        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _handler.HandleAsync(
                Arg.Any<IApplicationDbContext>(),
                Arg.Any<OperationRecord>(),
                Arg.Any<CancellationToken>())
            .Returns("result");
    }

    private void SetupDbSet(params OperationRecord[] records)
    {
        var mockDbSet = records.AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
    }

    private static OperationRecord MakePendingOperation(Guid id, OperationTypeEnum type = OperationTypeEnum.DataSync)
        => new() { Id = id, Type = type, Status = OperationStatusEnum.Pending, MaxRetries = 0, CreatedAt = DateTime.UtcNow };

    // guard: not found / cancelled

    [TestMethod]
    public async Task ProcessAsync_WhenOperationNotFound_Returns3()
    {
        SetupDbSet();

        var result = await _service.ProcessAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.AreEqual(-3, result);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenOperationIsCancelled_Returns3()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Status = OperationStatusEnum.Cancelled, MaxRetries = 0 });

        var result = await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual(-3, result);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenOperationNotFound_DoesNotSave()
    {
        SetupDbSet();

        await _service.ProcessAsync(Guid.NewGuid(), CancellationToken.None);

        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // guard: no handler / wrong status

    [TestMethod]
    public async Task ProcessAsync_WhenNoHandlerRegistered_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakePendingOperation(id));
        var serviceWithNoHandlers = new OperationProcessorService(_context, Enumerable.Empty<OperationHandler>());

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => serviceWithNoHandlers.ProcessAsync(id, CancellationToken.None));
    }

    [TestMethod]
    public async Task ProcessAsync_WhenOperationIsNotPending_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, Type = OperationTypeEnum.DataSync, Status = OperationStatusEnum.Running, MaxRetries = 0 });

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _service.ProcessAsync(id, CancellationToken.None));
    }

    // success path

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_Returns0()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakePendingOperation(id));

        var result = await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_SetsStatusToCompleted()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual(OperationStatusEnum.Completed, operation.Status);
    }

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_StoresHandlerResult()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);
        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Returns("""{"synced":142}""");

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual("""{"synced":142}""", operation.Result);
    }

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_SetsStartedAt()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.IsNotNull(operation.StartedAt);
    }

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_SetsCompletedAt()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.IsNotNull(operation.CompletedAt);
    }

    [TestMethod]
    public async Task ProcessAsync_OnSuccess_SavesChangesTwice()
    {
        // Once for StartOperation, once for CompleteOperation
        var id = Guid.NewGuid();
        SetupDbSet(MakePendingOperation(id));

        await _service.ProcessAsync(id, CancellationToken.None);

        await _context.Received(2).SaveChangesAsync(CancellationToken.None);
    }

    // failure path

    [TestMethod]
    public async Task ProcessAsync_WhenHandlerThrows_Returns4()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakePendingOperation(id));
        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("handler error"));

        var result = await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual(-4, result);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenHandlerThrows_SetsStatusToFailed()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);
        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("handler error"));

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual(OperationStatusEnum.Failed, operation.Status);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenHandlerThrows_StoresErrorMessage()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);
        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("handler error"));

        await _service.ProcessAsync(id, CancellationToken.None);

        Assert.AreEqual("handler error", operation.ErrorMessage);
    }

    // cancellation / timeout path

    [TestMethod]
    public async Task ProcessAsync_WhenCancelled_Returns5()
    {
        var id = Guid.NewGuid();
        SetupDbSet(MakePendingOperation(id));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException(cts.Token));

        var result = await _service.ProcessAsync(id, cts.Token);

        Assert.AreEqual(-5, result);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenCancelled_SetsStatusToTimedOut()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException(cts.Token));

        await _service.ProcessAsync(id, cts.Token);

        Assert.AreEqual(OperationStatusEnum.TimedOut, operation.Status);
    }

    [TestMethod]
    public async Task ProcessAsync_WhenCancelled_SetsIsTimedOutAndErrorMessage()
    {
        var id = Guid.NewGuid();
        var operation = MakePendingOperation(id);
        SetupDbSet(operation);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _handler.HandleAsync(Arg.Any<IApplicationDbContext>(), Arg.Any<OperationRecord>(), Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException(cts.Token));

        await _service.ProcessAsync(id, cts.Token);

        Assert.IsTrue(operation.IsTimedOut);
        Assert.AreEqual("Operation timed out.", operation.ErrorMessage);
    }
}
