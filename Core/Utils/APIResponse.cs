namespace RESTful_web_API_Course.Core;

public sealed class APIResponse {
    public Status? Status { get; set; }
    public object Data { get; set; }
}

public sealed class APIResponse<T> {
    public Status? Status { get; set; }
    public T? Data { get; set; }
}

public sealed class Status {
    public StatusCode StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Elapsed { get; private set; }
    public bool IsSuccessful { get; private set; }
    private DateTime OpenTimeStamp { get; init; }
    private DateTime CloseTimeStamp { get; set; }
    
    private Status () { }
    
    public static Builder Open() {
        return new Builder {
            _status = new Status {
                OpenTimeStamp = DateTime.UtcNow,
                StatusCode = StatusCode.OK,
                Message = string.Empty,
                Elapsed = 0
            }
        };
    }
    
    public class Builder {
        internal Status _status;
        
        internal Builder () { }

        public Builder WithCode(StatusCode code) {
            _status.StatusCode = code;
            return this;
        }

        public Builder IsSuccessful(bool success) {
            _status.IsSuccessful = success;
            return this;
        }
    
        public Builder WithMessage(string message) {
            _status.Message = message;
            return this;
        }

        public Status Close() {
            _status.CloseTimeStamp = DateTime.UtcNow;
            _status.Elapsed = (_status.CloseTimeStamp - _status.OpenTimeStamp).Milliseconds;
            return _status;
        }
    }
}
