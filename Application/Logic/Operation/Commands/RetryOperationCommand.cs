using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic.Operation.Commands;

public class RetryOperationCommand : IRequest<ResponseDto<Unit>>
{
    public Guid Id { get; set; }
}

public class RetryOperationCommandHandler : IRequestHandler<RetryOperationCommand, ResponseDto<Unit>>
{
    private readonly IApplicationDbContext Context;
    private readonly IOperationQueue Queue;

    public RetryOperationCommandHandler(IApplicationDbContext context, IOperationQueue queue)
    {
        Context = context;
        Queue = queue;
    }

    public async Task<ResponseDto<Unit>> Handle(RetryOperationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var operation = await Context.OperationRecords.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (operation == null)
            {
                return ResponseDto<Unit>.Failure(-1, "Operation not found");
            }
            
            if(operation.Status is OperationStatusEnum.Pending or OperationStatusEnum.Running or OperationStatusEnum.Completed)
            {
                return ResponseDto<Unit>.Failure(-2, "Operation is already pending, running or completed");
            }

            if(operation.RetryCount >= operation.MaxRetries)
            {
                return ResponseDto<Unit>.Failure(-3, "Maximum retry attempts reached");
            }
            
            operation.RetryCount++;
            operation.Status = OperationStatusEnum.Pending;
            operation.IsTimedOut = false;
            await Context.SaveChangesAsync(cancellationToken);
            await Queue.EnqueueAsync(operation.Id, cancellationToken);
            return ResponseDto<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return ResponseDto<Unit>.Failure(-4, ex.ToString());
        }
    }
}

