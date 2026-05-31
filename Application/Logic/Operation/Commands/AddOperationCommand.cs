using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Logic.Operation.Commands;

public class AddOperationCommand : IRequest<ResponseDto<Guid>>
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int MaxRetries { get; set; }
}

public class AddOperationCommandHandler : IRequestHandler<AddOperationCommand, ResponseDto<Guid>>
{
    private readonly IApplicationDbContext Context;
    private readonly IOperationQueue Queue;

    public AddOperationCommandHandler(IApplicationDbContext context, IOperationQueue queue)
    {
        Context = context;
        Queue = queue;
    }

    public async Task<ResponseDto<Guid>> Handle(AddOperationCommand request, CancellationToken cancellationToken)
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
                Progress = 0,
                MaxRetries = request.MaxRetries,
            };

            Context.OperationRecords.Add(newOperation);
            await Context.SaveChangesAsync(cancellationToken);
            await Queue.EnqueueAsync(newOperation.Id, cancellationToken);
            return ResponseDto<Guid>.Success(newOperation.Id);
        }
        catch (Exception ex)
        {
            return ResponseDto<Guid>.Failure(-4, ex.ToString());
        }
    }
}
