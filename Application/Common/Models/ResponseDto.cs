using Application.Interfaces;

namespace Application.Common.Models;

public class ResponseDto<T> : IErrorResponseDto
{
    public T? Data { get; }
    public bool Succeeded { get; }
    public int Code { get; set; }
    public string? Message { get; set; }
    
    protected ResponseDto(bool succeeded, int code, string? message)
    {
        Succeeded = succeeded;
        Code = code;
        Message = message;
    }
    
    protected ResponseDto(bool succeeded, int code)
    {
        Succeeded = succeeded;
        Code = code;
    }

    protected ResponseDto(bool succeeded, T? data)
    {
        Succeeded = succeeded;
        Data = data;
    }
    
    public static ResponseDto<T> Success(T? data)
    {
        return new ResponseDto<T>(true, data);
    }
    
    public static ResponseDto<T> Failure(int errorCode, string? errorMessage)
    {
        return new ResponseDto<T>(false, errorCode, errorMessage);
    }
    
    public static ResponseDto<T> Failure(int errorCode)
    {
        return new ResponseDto<T>(false, errorCode);
    }
}