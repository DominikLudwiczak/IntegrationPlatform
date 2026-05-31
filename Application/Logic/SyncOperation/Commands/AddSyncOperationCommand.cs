using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Logic.SyncOperation.Commands;

public class AddSyncOperationCommand : IRequest<ResponseDto<Unit>>
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int TimeoutInSeconds { get; set; }
}

public class AddSyncOperationCommandHandler : IRequestHandler<AddSyncOperationCommand, ResponseDto<Unit>>
{
    private readonly IApplicationDbContext Context;
    private readonly IOperationProcessor OperationProcessor;

    public AddSyncOperationCommandHandler(IApplicationDbContext context, IOperationProcessor operationProcessor)
    {
        Context = context;
        OperationProcessor = operationProcessor;
    }

    public async Task<ResponseDto<Unit>> Handle(AddSyncOperationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var newOperation = new OperationRecord()
            {
                Id = new Guid(),
                Type = request.Type,
                Payload = request.Payload,
                Status = OperationStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
                MaxRetries = 0,
            };
            
            Context.OperationRecords.Add(newOperation);
            await Context.SaveChangesAsync(cancellationToken);
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(request.TimeoutInSeconds * 1000);
            var returnCode = await OperationProcessor.ProcessAsync(newOperation.Id, cts.Token);
            if (returnCode != 0)
            {
                return ResponseDto<Unit>.Failure(returnCode);
            }
            return ResponseDto<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return ResponseDto<Unit>.Failure(-4, ex.ToString());
        }
    }
}
