namespace RESTful_web_API_Course.Core; 

public sealed class ServiceResponse {
    public object? Data { get; set; } = null;
    public StatusCode StatusCode { get; set; } = StatusCode.OK;
    public string Message { get; set; } = string.Empty;
}

public sealed class ServiceResponse<T> {
    public T? Data { get; set; } = default;
    public StatusCode StatusCode { get; set; } = StatusCode.OK;
    public string Message { get; set; } = string.Empty;
}