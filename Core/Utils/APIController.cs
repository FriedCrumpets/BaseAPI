using Microsoft.AspNetCore.Mvc;

namespace RESTful_web_API_Course.Core; 

public abstract class APIController : ControllerBase {

    public async Task<IActionResult> ExecuteAsync(Func<Task<ServiceResponse>> action) {
        var status = Status.Open();
        
        try {
            var result = await action.Invoke();
            
            return StatusCode((int)result.StatusCode, new APIResponse {
                Status = status
                    .WithCode(result.StatusCode)
                    .IsSuccessful(true)
                    .WithMessage(result.Message)
                    .Close(),
            });
        }
        catch (ServerException ex) {
            // known server Exceptions
            return StatusCode((int)ex.Code, new APIResponse {
                Status = status
                    .WithCode(ex.Code)
                    .IsSuccessful(false)
                    .WithMessage(ex.Message)
                    .Close()
            });
        }
        catch (Exception ex) {
            // anything else 
            return StatusCode(500, new APIResponse {
                Status = status
                    .WithCode((StatusCode)500)
                    .IsSuccessful(false)
                    .WithMessage(ex.Message)
                    .Close()
            });
        }
    }

    public async Task<IActionResult> ExecuteAsync<T>(Func<Task<ServiceResponse<T>>> action) {
        var status = Status.Open();
        
        try {
            var result = await action.Invoke();
            
            return StatusCode((int)result.StatusCode, new APIResponse<T> {
                Status = status
                    .WithCode(result.StatusCode)
                    .IsSuccessful(true)
                    .WithMessage(result.Message)
                    .Close(),
                Data = result.Data,
            });
        }
        catch (ServerException ex) {
            // known server Exceptions
            return StatusCode((int)ex.Code, new APIResponse<T> {
                Status = status
                    .WithCode(ex.Code)
                    .IsSuccessful(false)
                    .WithMessage(ex.Message)
                    .Close()
            });
        }
        catch (Exception ex) {
            // anything else 
            return StatusCode(500, new APIResponse<T> {
                Status = status
                    .WithCode((StatusCode)500)
                    .IsSuccessful(false)
                    .WithMessage(ex.Message)
                    .Close()
            });
        }
    }
}