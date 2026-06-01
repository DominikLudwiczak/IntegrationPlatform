using Application.Logic.SyncOperation.Commands;
using MockQueryable.NSubstitute;

namespace Application.Tests.Logic.SyncOperation.Commands;

[TestClass]
public class AddSyncOperationCommandHandlerTests
{
    private IApplicationDbContext _context = null!;
    private IOperationProcessor _processor = null!;
    private AddSyncOperationCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _processor = Substitute.For<IOperationProcessor>();
        _handler = new AddSyncOperationCommandHandler(_context, _processor);

        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(0);
    }

    [TestMethod]
    public async Task Handle_ProcessorReturnsZero_ReturnsSuccess()
    {
        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(0);
        var command = new AddSyncOperationCommand { Type = OperationTypeEnum.DataSync, TimeoutInSeconds = 30 };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data);
    }

    [TestMethod]
    public async Task Handle_ProcessorReturnsErrorCode_ReturnsFailureWithThatCode()
    {
        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(-5);
        var command = new AddSyncOperationCommand { Type = OperationTypeEnum.DataSync, TimeoutInSeconds = 30 };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-5, result.Code);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_SavesBeforeProcessing()
    {
        var saveOrder = 0;
        var processOrder = 0;
        var counter = 0;

        _context.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                saveOrder = ++counter;
                return Task.FromResult(1);
            });
        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                processOrder = ++counter;
                return Task.FromResult(0);
            });

        await _handler.Handle(new AddSyncOperationCommand { Type = OperationTypeEnum.DataSync, TimeoutInSeconds = 30 }, CancellationToken.None);

        Assert.IsTrue(saveOrder < processOrder, "SaveChangesAsync must be called before ProcessAsync");
    }

    [TestMethod]
    public async Task Handle_ValidCommand_CreatesOperationWithCorrectProperties()
    {
        var mockDbSet = new List<OperationRecord>().AsQueryable().BuildMockDbSet();
        _context.OperationRecords.Returns(mockDbSet);

        OperationRecord? captured = null;
        mockDbSet.When(x => x.Add(Arg.Any<OperationRecord>()))
            .Do(x => captured = x.Arg<OperationRecord>());

        var command = new AddSyncOperationCommand { Type = OperationTypeEnum.WebhookDispatch, Payload = "body", TimeoutInSeconds = 10 };

        await _handler.Handle(command, CancellationToken.None);

        Assert.IsNotNull(captured);
        Assert.AreEqual(OperationTypeEnum.WebhookDispatch, captured.Type);
        Assert.AreEqual("body", captured.Payload);
        Assert.AreEqual(OperationStatusEnum.Pending, captured.Status);
        Assert.AreEqual(0, captured.MaxRetries);
        Assert.AreEqual(0, captured.Progress);
    }

    [TestMethod]
    public async Task Handle_WhenProcessorThrows_ReturnsFailureWithCode4()
    {
        _processor.ProcessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Throws(new Exception("Processor error"));
        var command = new AddSyncOperationCommand { Type = OperationTypeEnum.DataSync, TimeoutInSeconds = 30 };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-4, result.Code);
    }

    [TestMethod]
    public async Task Handle_WhenSaveChangesThrows_ReturnsFailureWithCode4()
    {
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new Exception("DB error"));
        var command = new AddSyncOperationCommand { Type = OperationTypeEnum.DataSync, TimeoutInSeconds = 30 };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(-4, result.Code);
    }
}
