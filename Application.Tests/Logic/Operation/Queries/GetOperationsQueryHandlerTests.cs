using Application.Logic.Operation.Queries.GetOperations;
using MockQueryable.NSubstitute;

namespace Application.Tests.Logic.Operation.Queries;

[TestClass]
public class GetOperationsQueryHandlerTests
{
    private IApplicationDbContext _context = null!;
    private GetOperationsQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new GetOperationsQueryHandler(_context);
    }

    private void SetupDbSet(params OperationRecord[] records)
    {
        var mockDbSet = records.AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
    }

    [TestMethod]
    public async Task Handle_EmptyDatabase_ReturnsEmptyPaginatedList()
    {
        SetupDbSet();

        var result = await _handler.Handle(new GetOperationsQuery(), CancellationToken.None);

        Assert.AreEqual(0, result.TotalCount);
        Assert.AreEqual(0, result.Items.Count);
    }

    [TestMethod]
    public async Task Handle_NullPageParams_UsesDefaults()
    {
        SetupDbSet();

        var result = await _handler.Handle(new GetOperationsQuery { PageNumber = null, PageSize = null }, CancellationToken.None);

        Assert.AreEqual(1, result.PageNumber);
        Assert.AreEqual(0, result.TotalPages);
    }

    [TestMethod]
    public async Task Handle_CustomPagination_AppliesCorrectly()
    {
        var records = Enumerable.Range(0, 10)
            .Select(i => new OperationRecord { Id = Guid.NewGuid(), MaxRetries = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-i) })
            .ToArray();
        SetupDbSet(records);

        var result = await _handler.Handle(new GetOperationsQuery { PageNumber = 2, PageSize = 3 }, CancellationToken.None);

        Assert.AreEqual(10, result.TotalCount);
        Assert.AreEqual(4, result.TotalPages);
        Assert.AreEqual(2, result.PageNumber);
        Assert.AreEqual(3, result.Items.Count);
    }

    [TestMethod]
    public async Task Handle_SingleRecord_ReturnsCorrectlyMappedItem()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var record = new OperationRecord
        {
            Id = id,
            Type = OperationTypeEnum.WebhookDispatch,
            Status = OperationStatusEnum.Running,
            CreatedAt = now,
            MaxRetries = 2,
            RetryCount = 1,
            IsTimedOut = false,
        };
        SetupDbSet(record);

        var result = await _handler.Handle(new GetOperationsQuery(), CancellationToken.None);

        Assert.AreEqual(1, result.Items.Count);
        var item = result.Items[0];
        Assert.AreEqual(id, item.Id);
        Assert.AreEqual(OperationTypeEnum.WebhookDispatch, item.Type);
        Assert.AreEqual(OperationStatusEnum.Running, item.Status);
        Assert.AreEqual(now, item.CreatedAt);
        Assert.AreEqual(2, item.MaxRetries);
        Assert.AreEqual(1, item.RetryCount);
        Assert.IsFalse(item.IsTimedOut);
    }

    [TestMethod]
    public async Task Handle_MultipleRecords_ReturnsOrderedByCreatedAtDescending()
    {
        var oldest = new OperationRecord { Id = Guid.NewGuid(), MaxRetries = 0, CreatedAt = DateTime.UtcNow.AddHours(-2) };
        var middle = new OperationRecord { Id = Guid.NewGuid(), MaxRetries = 0, CreatedAt = DateTime.UtcNow.AddHours(-1) };
        var newest = new OperationRecord { Id = Guid.NewGuid(), MaxRetries = 0, CreatedAt = DateTime.UtcNow };
        SetupDbSet(oldest, middle, newest);

        var result = await _handler.Handle(new GetOperationsQuery { PageSize = 10 }, CancellationToken.None);

        Assert.AreEqual(3, result.Items.Count);
        Assert.AreEqual(newest.Id, result.Items[0].Id);
        Assert.AreEqual(middle.Id, result.Items[1].Id);
        Assert.AreEqual(oldest.Id, result.Items[2].Id);
    }

    [TestMethod]
    public async Task Handle_PageSizeOf1_ReturnsSingleItem()
    {
        var records = Enumerable.Range(0, 5)
            .Select(_ => new OperationRecord { Id = Guid.NewGuid(), MaxRetries = 0 })
            .ToArray();
        SetupDbSet(records);

        var result = await _handler.Handle(new GetOperationsQuery { PageNumber = 1, PageSize = 1 }, CancellationToken.None);

        Assert.AreEqual(1, result.Items.Count);
        Assert.AreEqual(5, result.TotalCount);
        Assert.AreEqual(5, result.TotalPages);
    }
}
