using Application.Interfaces;

namespace Infrastructure;

public class ErrorSimulator : IErrorSimulator
{
    private readonly Random _random = new();
    
    public bool ShouldSimulateError()
    {
        return _random.Next(0, 10) < 2;
    }
}
