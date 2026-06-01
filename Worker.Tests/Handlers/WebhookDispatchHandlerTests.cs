using MockQueryable.NSubstitute;
using Worker.Handlers;

namespace Worker.Tests.Handlers;

[TestClass]
public class WebhookDispatchHandlerTests
{
    private IApplicationDbContext _context = null!;
    private IErrorSimulator _errorSimulator = null!;
    private WebhookDispatchHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _errorSimulator = Substitute.For<IErrorSimulator>();
        _handler = new WebhookDispatchHandler(_errorSimulator);

        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [TestMethod]
    public void Constructor_SetsWebhookDispatchOperationType()
    {
        Assert.AreEqual(OperationTypeEnum.WebhookDispatch, _handler.OperationType);
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

    [TestMethod]
    public async Task HandleAsync_WhenCancelled_ProgressRemainsZero()
    {
        var operation = new OperationRecord { MaxRetries = 0 };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try { await _handler.HandleAsync(_context, operation, cts.Token); } catch (OperationCanceledException) { }

        Assert.AreEqual(0, operation.Progress);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task HandleAsync_OnSuccess_ReturnsExpectedJson()
    {
        _errorSimulator.ShouldSimulateError().Returns(false);
        var operation = new OperationRecord { MaxRetries = 0 };

        var result = await _handler.HandleAsync(_context, operation, CancellationToken.None);

        Assert.AreEqual("""{"status":200,"delivered":true}""", result);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task HandleAsync_OnSuccess_SetsProgressTo100()
    {
        _errorSimulator.ShouldSimulateError().Returns(false);
        var operation = new OperationRecord { MaxRetries = 0 };

        await _handler.HandleAsync(_context, operation, CancellationToken.None);

        Assert.AreEqual(100, operation.Progress);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task HandleAsync_WhenErrorSimulated_ThrowsException()
    {
        _errorSimulator.ShouldSimulateError().Returns(true);
        var operation = new OperationRecord { MaxRetries = 0 };

        await Assert.ThrowsExceptionAsync<Exception>(
            () => _handler.HandleAsync(_context, operation, CancellationToken.None));
    }
}
