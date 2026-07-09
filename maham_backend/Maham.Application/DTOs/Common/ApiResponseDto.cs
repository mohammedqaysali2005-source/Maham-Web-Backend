using System.Collections.Generic;

namespace Maham.Application.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiResponseDto<T> SuccessResponse(T data, string message = "OK")
    {
        return new ApiResponseDto<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponseDto<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T> { Success = false, Data = default, Message = message, Errors = errors };
    }
}
