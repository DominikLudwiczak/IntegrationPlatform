using Consumer.Common.Models;

namespace Consumer.Common.Interfaces;

public interface IOperationService
{
    Task AddOperation(AddAsyncOperationRequest request);
    Task<PaginatedListDto<GetOperationsResponseDto>> GetOperations(int pageNumber, int pageSize);
}