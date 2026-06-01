using Application.Common.Models;
using Application.Logic.Operation.Commands;
using MockQueryable.NSubstitute;

namespace Application.Tests.Logic.Operation.Commands;

[TestClass]
public class AddOperationCommandHandlerTests
{
    private IApplicationDbContext _context = null!;
    private IOperationQueue _queue = null!;
    private AddOperationCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _queue = Substitute.For<IOperationQueue>();
        _handler = new AddOperationCommandHandler(_context, _queue);

        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var command = new AddOperationCommand { Type = OperationTypeEnum.DataSync, Payload = "data", MaxRetries = 3 };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(0, result.Code);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_CreatesOperationWithCorrectProperties()
    {
        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);

        OperationRecord? captured = null;
        mockDbSet.When(x => x.Add(Arg.Any<OperationRecord>()))
            .Do(x => captured = x.Arg<OperationRecord>());

        var command = new AddOperationCommand { Type = OperationTypeEnum.ReportGeneration, Payload = "payload", MaxRetries = 2 };

        await _handler.Handle(command, CancellationToken.None);

        Assert.IsNotNull(captured);
        Assert.AreEqual(OperationTypeEnum.ReportGeneration, captured.Type);
        Assert.AreEqual("payload", captured.Payload);
        Assert.AreEqual(OperationStatusEnum.Pending, captured.Status);
        Assert.AreEqual(0, captured.Progress);
        Assert.AreEqual(2, captured.MaxRetries);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_SavesChangesAndEnqueues()
    {
        var command = new AddOperationCommand { Type = OperationTypeEnum.DataSync };

        await _handler.Handle(command, CancellationToken.None);

        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _queue.Received(1).EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Handle_WhenSaveChangesThrows_ReturnsFailureWithCode4()
    {
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new Exception("DB error"));
        var command = new AddOperationCommand { Type = OperationTypeEnum.DataSync };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-4, result.Code);
    }

    [TestMethod]
    public async Task Handle_WhenEnqueueThrows_ReturnsFailureWithCode4()
    {
        _queue.EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Throws(new Exception("Queue error"));
        var command = new AddOperationCommand { Type = OperationTypeEnum.DataSync };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-4, result.Code);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_DoesNotEnqueueBeforeSaving()
    {
        var saveOrder = 0;
        var enqueueOrder = 0;
        var counter = 0;

        _context.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                saveOrder = ++counter;
                return Task.FromResult(1);
            });
        _queue.EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                enqueueOrder = ++counter;
                return Task.CompletedTask;
            });

        await _handler.Handle(new AddOperationCommand { Type = OperationTypeEnum.DataSync }, CancellationToken.None);

        Assert.IsTrue(saveOrder < enqueueOrder, "SaveChangesAsync must be called before EnqueueAsync");
    }
}
