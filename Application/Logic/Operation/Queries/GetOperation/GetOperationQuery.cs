using Application.Common.Models;
using Application.Common.Models.Operation;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic.Operation.Queries.GetOperation;

public class GetOperationQuery : IRequest<ResponseDto<OperationRecordDto>>
{
    public Guid Id { get; set; }
}

public class GetOperationQueryHandler : IRequestHandler<GetOperationQuery, ResponseDto<OperationRecordDto>>
{
    private readonly IApplicationDbContext Context;

    public GetOperationQueryHandler(IApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<ResponseDto<OperationRecordDto>> Handle(GetOperationQuery request, CancellationToken cancellationToken)
    {
        var operation = await Context.OperationRecords
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (operation != null)
        {
            var result = new OperationRecordDto()
            {
                Id = operation.Id,
                Type = operation.Type,
                Payload = operation.Payload,
                Status = operation.Status,
                Result = operation.Result,
                ErrorMessage = operation.ErrorMessage,
                CreatedAt = operation.CreatedAt,
                StartedAt = operation.StartedAt,
                CompletedAt = operation.CompletedAt,
                Progress = operation.Progress,
                ProcessingTimeMs = operation.ProcessingTimeMs,
                RetryCount = operation.RetryCount,
                MaxRetries = operation.MaxRetries,
                IsTimedOut = operation.IsTimedOut
            };
            return ResponseDto<OperationRecordDto>.Success(result);
        }
        return ResponseDto<OperationRecordDto>.Failure(-1);
    }
}
