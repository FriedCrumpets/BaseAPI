using Microsoft.AspNetCore.Identity.Data;
using RESTful_web_API_Course.Core;
using RESTful_web_API_Course.Features.Auth.Models;

namespace RESTful_web_API_Course.Features.Auth.Services; 

public interface IAuthService {
    public Task<ServiceResponse> Register(RegisterRequest registration, HttpContext context);
    public Task<ServiceResponse> Login(AuthLoginRequestDTO login, bool? useCookies, bool? useSessionCookies);
    // public Task<ServiceResponse> Refresh(RefreshRequest refreshRequest);
    // public Task<ServiceResponse> ConfirmEmail(string userId, string code, string? changedEmail);
    // public Task<ServiceResponse> ResendConfirmationEmail(ResendConfirmationEmailRequest resendRequest, HttpContext context);
    // public Task<ServiceResponse> ForgotPassword(ForgotPasswordRequest resetRequest);
    // public Task<ServiceResponse> ResetPassword(ResetPasswordRequest resetRequest);
    // public Task<ServiceResponse> Manage2FA(ClaimsPrincipal claimsPrincipal, TwoFactorRequest tfaRequest);
    // public Task<ServiceResponse> ManageInfo(ClaimsPrincipal claimsPrincipal);
    // public Task<ServiceResponse> ManageInfo(ClaimsPrincipal claimsPrincipal, InfoRequest infoRequest, HttpContext context);
}