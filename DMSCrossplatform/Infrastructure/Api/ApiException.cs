using System;
using System.Net;

namespace DMSCrossplatform.Infrastructure.Api;

public class ApiException: Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }
    
    public ApiException(HttpStatusCode statusCode, string message, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}