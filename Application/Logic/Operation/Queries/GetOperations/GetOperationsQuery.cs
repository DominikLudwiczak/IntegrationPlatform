using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Logic.Operation.Queries.GetOperations;

public class GetOperationsQuery : IRequest<PaginatedListDto<GetOperationsResponseDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}

public class GetOperationsQueryHandler : IRequestHandler<GetOperationsQuery, PaginatedListDto<GetOperationsResponseDto>>
{
    private readonly IApplicationDbContext Context;

    public GetOperationsQueryHandler(IApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<PaginatedListDto<GetOperationsResponseDto>> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize ?? 25;
        var page = request.PageNumber ?? 1;

        var operations = Context.OperationRecords
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable();
        var list = await PaginatedListDto<OperationRecord>.CreateAsync(operations, page, pageSize);

        return new PaginatedListDto<GetOperationsResponseDto>(list.Items.Select(x => new GetOperationsResponseDto()
        {
            Id = x.Id,
            Type = x.Type,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt,
            Progress = x.Progress,
            ProcessingTimeMs = x.ProcessingTimeMs,
            RetryCount = x.RetryCount,
            MaxRetries = x.MaxRetries,
            IsTimedOut = x.IsTimedOut,
        }).ToList(), list.TotalCount, list.PageNumber, pageSize);
    }
}
