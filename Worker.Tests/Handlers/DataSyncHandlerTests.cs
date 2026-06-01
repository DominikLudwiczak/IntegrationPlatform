using MockQueryable.NSubstitute;
using Worker.Handlers;

namespace Worker.Tests.Handlers;

[TestClass]
public class DataSyncHandlerTests
{
    private IApplicationDbContext _context = null!;
    private IErrorSimulator _errorSimulator = null!;
    private DataSyncHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _errorSimulator = Substitute.For<IErrorSimulator>();
        _handler = new DataSyncHandler(_errorSimulator);

        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [TestMethod]
    public void Constructor_SetsDataSyncOperationType()
    {
        Assert.AreEqual(OperationTypeEnum.DataSync, _handler.OperationType);
    }

    [TestMethod]
    public async Task HandleAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var operation = new OperationRecord { MaxRetries = 0 };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(
            () => _handler.HandleAsync(_context, operation, cts.Token));
    }

    // With a pre-cancelled token, MakeProgress(10) completes (it uses CancellationToken.None internally),
    // then the first Task.Delay throws immediately — so progress is 10 before the exception.
    [TestMethod]
    public async Task HandleAsync_WhenCancelled_SetsInitialProgressTo10BeforeThrow()
    {
        var operation = new OperationRecord { MaxRetries = 0 };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try { await _handler.HandleAsync(_context, operation, cts.Token); } catch (OperationCanceledException) { }

        Assert.AreEqual(10, operation.Progress);
        await _context.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
