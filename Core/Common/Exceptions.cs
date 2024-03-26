namespace RESTful_web_API_Course.Core; 

public class ServerException(StatusCode code, string message) : Exception(message) {
    public StatusCode Code { get; } = code;
}

public class InternalServerException : ServerException {
    public InternalServerException(string message) : base(StatusCode.InternalServerError, message) { }
}

public class NotImplementedException : ServerException {
    public NotImplementedException(string message) : base(StatusCode.NotImplemented, message) { }
}

public class BadGatewayException : ServerException {
    public BadGatewayException(string message) : base(StatusCode.BadGateway, message) { }
}

public class ServiceUnavailableException : ServerException {
    public ServiceUnavailableException(string message) : base(StatusCode.ServiceUnavailable, message) { }
}

public class GatewayTimeoutException : ServerException {
    public GatewayTimeoutException(string message) : base(StatusCode.GatewayTimeout, message) { }
}

public class HttpVersionNotsupportedException : ServerException {
    public HttpVersionNotsupportedException(string message) : base(StatusCode.HttpVersionNotsupported, message) { }
}

public class VariantAlsoNegotiatesException : ServerException {
    public VariantAlsoNegotiatesException(string message) : base(StatusCode.VariantAlsoNegotiates, message) { }
}

public class InsufficientStorageException : ServerException {
    public InsufficientStorageException(string message) : base(StatusCode.InsufficientStorage, message) { }
}

public class LoopDetectedException : ServerException {
    public LoopDetectedException(string message) : base(StatusCode.LoopDetected, message) { }
}

public class NotExtendedException : ServerException {
    public NotExtendedException(string message) : base(StatusCode.NotExtended, message) { }
}

public class NetworkAuthenticationRequiredException : ServerException {
    public NetworkAuthenticationRequiredException(string message) : base(StatusCode.NetworkAuthenticationRequired, message) { }
}
