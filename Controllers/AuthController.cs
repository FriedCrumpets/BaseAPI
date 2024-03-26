using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RESTful_web_API_Course.Core;
using RESTful_web_API_Course.Features.Auth.Models;
using RESTful_web_API_Course.Features.Auth.Services;

namespace RESTful_web_API_Course.Controllers; 

[ApiController, Route("api/[controller]")]
public class AuthController(IAuthService service) : APIController {
    public IAuthService Service { get; } = service;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registration) {
        return await ExecuteAsync(() => Service.Register(registration, HttpContext));
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequestDTO login, [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies) {
        return await ExecuteAsync(() => Service.Login(login, useCookies, useSessionCookies));
    }
    //
    // [HttpPost, Route("/refresh")]
    // public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest) {
    //     return await ExecuteAsync(() => Service.Refresh(refreshRequest));
    // }
    //
    // [HttpGet, Route("/confirmEmail")]
    // public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code,
    //     [FromQuery] string? changedEmail) {
    //     return await ExecuteAsync(() => Service.ConfirmEmail(userId, code, changedEmail));
    // }
    //
    // [HttpPost, Route("/resendConfirmationEmail")]
    // public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest) {
    //     return await ExecuteAsync(() => Service.ResendConfirmationEmail(resendRequest, HttpContext));
    // }
    //
    // [HttpPost, Route("/forgotPassword")]
    // public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest) {
    //     return await ExecuteAsync(() => Service.ForgotPassword(resetRequest));
    // }
    //
    // [HttpPost, Route("/resetPassword")]
    // public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest) {
    //     return await ExecuteAsync(() => Service.ResetPassword(resetRequest));
    // }
    //
    // [Authorize]
    // [HttpPost, Route("manage/2fa")]
    // public async Task<IActionResult> Manage2FA(ClaimsPrincipal claimsPrincipal, [FromBody] TwoFactorRequest tfaRequest) {
    //     return await ExecuteAsync(() => Service.Manage2FA(claimsPrincipal, tfaRequest));
    // }
    //
    // [Authorize]
    // [HttpGet, Route("manage/info")]
    // public async Task<IActionResult> ManageInfo(ClaimsPrincipal claimsPrincipal) {
    //     return await ExecuteAsync(() => Service.ManageInfo(claimsPrincipal));
    // }
    //
    // [Authorize]
    // [HttpPost, Route("manage/info")]
    // public async Task<IActionResult> ManageInfo(ClaimsPrincipal claimsPrincipal, [FromBody] InfoRequest infoRequest) {
    //     return await ExecuteAsync(() => Service.ManageInfo(claimsPrincipal, infoRequest, HttpContext));
    // }
}