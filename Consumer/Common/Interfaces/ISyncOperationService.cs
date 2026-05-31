using Consumer.Common.Models;

namespace Consumer.Common.Interfaces;

public interface ISyncOperationService
{
    Task<Guid> AddSyncOperation(AddSyncOperationRequest request);
}