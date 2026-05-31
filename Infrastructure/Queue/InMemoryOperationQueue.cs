using System.Threading.Channels;
using Application.Interfaces;

namespace Infrastructure.Queue;

public class InMemoryOperationQueue : IOperationQueue
{
    private readonly Channel<Guid> _channel;

    public InMemoryOperationQueue(int capacity = 1000)
    {
        _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = false
        });
    }
    
    public async Task EnqueueAsync(Guid operationId, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(operationId, ct);

    public async Task<Guid?> DequeueAsync(CancellationToken ct = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}