namespace Application.Interfaces;

public interface IErrorResponseDto
{
    public int Code { get; }
    public string? Message { get; }
}