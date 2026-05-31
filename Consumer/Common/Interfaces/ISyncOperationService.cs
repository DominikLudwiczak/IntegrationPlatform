using Consumer.Common.Models;

namespace Consumer.Common.Interfaces;

public interface ISyncOperationService
{
    Task AddSyncOperation(AddSyncOperationRequest request);
}