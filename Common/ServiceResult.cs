using System;

namespace Common;

public class Error
{
    public string Key { get; set; }
    public List<string> Messages { get; set; }
}
public class ServiceResult<T>
where T : class, new() //new() requires the be used a constructor without parameters
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public List<Error> Errors { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Errors = null
        };
    }
    public static ServiceResult<T> Failure(T data, List<Error> errors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Data = data,
            Errors = errors
        };
    }
}