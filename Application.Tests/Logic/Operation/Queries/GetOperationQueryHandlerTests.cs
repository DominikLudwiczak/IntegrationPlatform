using Application.Common.Models.Operation;
using Application.Logic.Operation.Queries.GetOperation;
using MockQueryable.NSubstitute;

namespace Application.Tests.Logic.Operation.Queries;

[TestClass]
public class GetOperationQueryHandlerTests
{
    private IApplicationDbContext _context = null!;
    private GetOperationQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new GetOperationQueryHandler(_context);
    }

    private void SetupDbSet(params OperationRecord[] records)
    {
        var mockDbSet = records.AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
    }

    [TestMethod]
    public async Task Handle_OperationNotFound_ReturnsFailureCode1()
    {
        SetupDbSet();

        var result = await _handler.Handle(new GetOperationQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-1, result.Code);
    }

    [TestMethod]
    public async Task Handle_OperationFound_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        SetupDbSet(new OperationRecord { Id = id, MaxRetries = 3 });

        var result = await _handler.Handle(new GetOperationQuery { Id = id }, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data);
    }

    [TestMethod]
    public async Task Handle_OperationFound_MapsAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var started = now.AddSeconds(-10);
        var completed = now.AddSeconds(-1);

        var operation = new OperationRecord
        {
            Id = id,
            Type = OperationTypeEnum.ReportGeneration,
            Payload = "test-payload",
            Status = OperationStatusEnum.Completed,
            Result = "ok",
            ErrorMessage = null,
            CreatedAt = now,
            StartedAt = started,
            CompletedAt = completed,
            MaxRetries = 5,
            RetryCount = 2,
            ProcessingTimeMs = 9000,
            IsTimedOut = false,
        };
        SetupDbSet(operation);

        var result = await _handler.Handle(new GetOperationQuery { Id = id }, CancellationToken.None);

        var dto = result.Data!;
        Assert.AreEqual(id, dto.Id);
        Assert.AreEqual(OperationTypeEnum.ReportGeneration, dto.Type);
        Assert.AreEqual("test-payload", dto.Payload);
        Assert.AreEqual(OperationStatusEnum.Completed, dto.Status);
        Assert.AreEqual("ok", dto.Result);
        Assert.IsNull(dto.ErrorMessage);
        Assert.AreEqual(now, dto.CreatedAt);
        Assert.AreEqual(started, dto.StartedAt);
        Assert.AreEqual(completed, dto.CompletedAt);
        Assert.AreEqual(0, dto.Progress);
        Assert.AreEqual(9000, dto.ProcessingTimeMs);
        Assert.AreEqual(2, dto.RetryCount);
        Assert.AreEqual(5, dto.MaxRetries);
        Assert.IsFalse(dto.IsTimedOut);
    }

    [TestMethod]
    public async Task Handle_OperationNotFound_DataIsNull()
    {
        SetupDbSet();

        var result = await _handler.Handle(new GetOperationQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.IsNull(result.Data);
    }
}
