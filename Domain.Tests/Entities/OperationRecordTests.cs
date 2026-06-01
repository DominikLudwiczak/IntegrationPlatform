using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities;

[TestClass]
public class OperationRecordTests
{
    [TestMethod]
    public void Constructor_InitializesWithDefaultValues()
    {
        var record = new OperationRecord();

        Assert.AreEqual(default(Guid), record.Id);
        Assert.AreEqual(0, record.Progress);
        Assert.AreEqual(0, record.RetryCount);
    }

    [TestMethod]
    public void Progress_SetToValidValue_Succeeds()
    {
        var record = new OperationRecord { Progress = 50 };
        Assert.AreEqual(50, record.Progress);
    }

    [TestMethod]
    public void Progress_SetToZero_Succeeds()
    {
        var record = new OperationRecord { Progress = 0 };
        Assert.AreEqual(0, record.Progress);
    }

    [TestMethod]
    public void Progress_SetTo100_Succeeds()
    {
        var record = new OperationRecord { Progress = 100 };
        Assert.AreEqual(100, record.Progress);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Progress_SetToNegative_ThrowsException()
    {
        var record = new OperationRecord { Progress = -1 };
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Progress_SetAbove100_ThrowsException()
    {
        var record = new OperationRecord { Progress = 101 };
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Progress_SetTo1000_ThrowsException()
    {
        var record = new OperationRecord { Progress = 1000 };
    }

    [TestMethod]
    public void RetryCount_SetToValidValue_Succeeds()
    {
        var record = new OperationRecord { MaxRetries = 5, RetryCount = 3 };
        Assert.AreEqual(3, record.RetryCount);
    }

    [TestMethod]
    public void RetryCount_SetToZero_Succeeds()
    {
        var record = new OperationRecord { MaxRetries = 5, RetryCount = 0 };
        Assert.AreEqual(0, record.RetryCount);
    }

    [TestMethod]
    public void RetryCount_SetEqualToMaxRetries_Succeeds()
    {
        var record = new OperationRecord { MaxRetries = 5, RetryCount = 5 };
        Assert.AreEqual(5, record.RetryCount);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RetryCount_SetGreaterThanMaxRetries_ThrowsException()
    {
        var record = new OperationRecord { MaxRetries = 5, RetryCount = 6 };
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RetryCount_SetToNegative_ThrowsException()
    {
        var record = new OperationRecord { MaxRetries = 5, RetryCount = -1 };
    }

    [TestMethod]
    public void MultipleProgressUpdates_AllWithinRange_SucceedsForEach()
    {
        var record = new OperationRecord();

        record.Progress = 10;
        Assert.AreEqual(10, record.Progress);

        record.Progress = 50;
        Assert.AreEqual(50, record.Progress);

        record.Progress = 100;
        Assert.AreEqual(100, record.Progress);
    }

    [TestMethod]
    public void OperationType_CanBeSet()
    {
        var record = new OperationRecord { Type = OperationTypeEnum.DataSync };
        Assert.AreEqual(OperationTypeEnum.DataSync, record.Type);
    }

    [TestMethod]
    public void Status_CanBeSet()
    {
        var record = new OperationRecord { Status = OperationStatusEnum.Running };
        Assert.AreEqual(OperationStatusEnum.Running, record.Status);
    }

    [TestMethod]
    public void CreatedAt_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var record = new OperationRecord { CreatedAt = now };
        Assert.AreEqual(now, record.CreatedAt);
    }

    [TestMethod]
    public void Payload_CanBeSetToNull()
    {
        var record = new OperationRecord { Payload = null };
        Assert.IsNull(record.Payload);
    }

    [TestMethod]
    public void Payload_CanBeSet()
    {
        var json = """{"key":"value"}""";
        var record = new OperationRecord { Payload = json };
        Assert.AreEqual(json, record.Payload);
    }

    [TestMethod]
    public void Result_CanBeSet()
    {
        var result = """{"success":true}""";
        var record = new OperationRecord { Result = result };
        Assert.AreEqual(result, record.Result);
    }

    [TestMethod]
    public void ErrorMessage_CanBeSet()
    {
        var error = "Operation failed";
        var record = new OperationRecord { ErrorMessage = error };
        Assert.AreEqual(error, record.ErrorMessage);
    }

    [TestMethod]
    public void IsTimedOut_DefaultsToFalse()
    {
        var record = new OperationRecord();
        Assert.IsFalse(record.IsTimedOut);
    }

    [TestMethod]
    public void IsTimedOut_CanBeSetToTrue()
    {
        var record = new OperationRecord { IsTimedOut = true };
        Assert.IsTrue(record.IsTimedOut);
    }

    [TestMethod]
    public void ProcessingTimeMs_CanBeNull()
    {
        var record = new OperationRecord();
        Assert.IsNull(record.ProcessingTimeMs);
    }

    [TestMethod]
    public void ProcessingTimeMs_CanBeSet()
    {
        var record = new OperationRecord { ProcessingTimeMs = 5000 };
        Assert.AreEqual(5000, record.ProcessingTimeMs);
    }

    [TestMethod]
    public void StartedAt_CanBeNull()
    {
        var record = new OperationRecord();
        Assert.IsNull(record.StartedAt);
    }

    [TestMethod]
    public void StartedAt_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var record = new OperationRecord { StartedAt = now };
        Assert.AreEqual(now, record.StartedAt);
    }

    [TestMethod]
    public void CompletedAt_CanBeNull()
    {
        var record = new OperationRecord();
        Assert.IsNull(record.CompletedAt);
    }

    [TestMethod]
    public void CompletedAt_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var record = new OperationRecord { CompletedAt = now };
        Assert.AreEqual(now, record.CompletedAt);
    }

    [TestMethod]
    public void OperationRecord_CompleteWorkflow_Succeeds()
    {
        var record = new OperationRecord
        {
            Id = Guid.NewGuid(),
            Type = OperationTypeEnum.DataSync,
            Status = OperationStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
            Progress = 0,
            MaxRetries = 3
        };

        record.Progress = 25;
        record.Status = OperationStatusEnum.Running;
        record.StartedAt = DateTime.UtcNow;

        record.Progress = 50;
        record.Progress = 100;

        record.Status = OperationStatusEnum.Completed;
        record.CompletedAt = DateTime.UtcNow;
        record.Result = """{"synced":100}""";
        record.ProcessingTimeMs = 5000;

        Assert.AreEqual(OperationTypeEnum.DataSync, record.Type);
        Assert.AreEqual(OperationStatusEnum.Completed, record.Status);
        Assert.AreEqual(100, record.Progress);
        Assert.IsNotNull(record.Result);
        Assert.IsNotNull(record.CompletedAt);
    }
}
