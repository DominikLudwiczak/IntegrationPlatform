using Consumer.Common.Models;

namespace Consumer.Common.Interfaces;

public interface IOperationService
{
    Task<Guid> AddOperation(AddAsyncOperationRequest request);
    Task<PaginatedListDto<GetOperationsResponseDto>> GetOperations(int pageNumber, int pageSize);
}