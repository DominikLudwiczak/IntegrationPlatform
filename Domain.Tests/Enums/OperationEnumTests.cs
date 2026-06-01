using Domain.Enums;

namespace Domain.Tests.Enums;

[TestClass]
public class OperationTypeEnumTests
{
    [TestMethod]
    public void OperationTypeEnum_HasDataSyncValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationTypeEnum), OperationTypeEnum.DataSync));
    }

    [TestMethod]
    public void OperationTypeEnum_HasReportGenerationValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationTypeEnum), OperationTypeEnum.ReportGeneration));
    }

    [TestMethod]
    public void OperationTypeEnum_HasWebhookDispatchValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationTypeEnum), OperationTypeEnum.WebhookDispatch));
    }

    [TestMethod]
    public void OperationTypeEnum_HasExactlyThreeValues()
    {
        var values = Enum.GetValues(typeof(OperationTypeEnum));
        Assert.AreEqual(3, values.Length);
    }

    [TestMethod]
    public void OperationTypeEnum_CanConvertToString()
    {
        var type = OperationTypeEnum.DataSync;
        string str = type.ToString();
        Assert.AreEqual("DataSync", str);
    }

    [TestMethod]
    public void OperationTypeEnum_CanParseFromString()
    {
        var parsed = Enum.Parse<OperationTypeEnum>("DataSync");
        Assert.AreEqual(OperationTypeEnum.DataSync, parsed);
    }
}

[TestClass]
public class OperationStatusEnumTests
{
    [TestMethod]
    public void OperationStatusEnum_HasPendingValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.Pending));
    }

    [TestMethod]
    public void OperationStatusEnum_HasRunningValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.Running));
    }

    [TestMethod]
    public void OperationStatusEnum_HasCompletedValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.Completed));
    }

    [TestMethod]
    public void OperationStatusEnum_HasFailedValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.Failed));
    }

    [TestMethod]
    public void OperationStatusEnum_HasTimedOutValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.TimedOut));
    }

    [TestMethod]
    public void OperationStatusEnum_HasCancelledValue()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(OperationStatusEnum), OperationStatusEnum.Cancelled));
    }

    [TestMethod]
    public void OperationStatusEnum_HasExactlySixValues()
    {
        var values = Enum.GetValues(typeof(OperationStatusEnum));
        Assert.AreEqual(6, values.Length);
    }

    [TestMethod]
    public void OperationStatusEnum_CanConvertToString()
    {
        var status = OperationStatusEnum.Running;
        string str = status.ToString();
        Assert.AreEqual("Running", str);
    }

    [TestMethod]
    public void OperationStatusEnum_CanParseFromString()
    {
        var parsed = Enum.Parse<OperationStatusEnum>("Completed");
        Assert.AreEqual(OperationStatusEnum.Completed, parsed);
    }

    [TestMethod]
    public void OperationStatusEnum_AllStatusesAreAccessible()
    {
        var statuses = new[]
        {
            OperationStatusEnum.Pending,
            OperationStatusEnum.Running,
            OperationStatusEnum.Completed,
            OperationStatusEnum.Failed,
            OperationStatusEnum.TimedOut,
            OperationStatusEnum.Cancelled
        };

        foreach (var status in statuses)
        {
            Assert.IsNotNull(status);
        }
    }
}
