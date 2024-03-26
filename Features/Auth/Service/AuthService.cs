using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RESTful_web_API_Course.Core;
using RESTful_web_API_Course.Features.Auth.Models;

namespace RESTful_web_API_Course.Features.Auth.Services; 

public class AuthService<T> (
    TimeProvider timeProvider, 
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions, 
    IEmailSender<T> emailSender, 
    LinkGenerator linkGenerator,
    UserManager<T> userManager,
    IUserStore<T> userStore,
    SignInManager<T> signInManager) : IAuthService where T : IdentityUser, new() {
    
    public TimeProvider TimeProvider { get; } = timeProvider;
    public IOptionsMonitor<BearerTokenOptions> BearerTokenOptions { get; } = bearerTokenOptions;
    public IEmailSender<T> EmailSender { get; } = emailSender;
    public LinkGenerator LinkGenerator { get; } = linkGenerator;
    public UserManager<T> UserManager { get; } = userManager;
    public IUserStore<T> UserStore { get; } = userStore;
    public SignInManager<T> SignInManager { get; } = signInManager;
    
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    public async Task<ServiceResponse> Register(RegisterRequest registration, HttpContext context) {
        if (! UserManager.SupportsUserEmail) {
            throw new NotSupportedException($"{nameof(AuthService<T>)} requires a user store with email support.");
        }

        var emailStore = (IUserEmailStore<T>)UserStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !EmailAddressAttribute.IsValid(email)) {
            return new ServiceResponse {
                Data = CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email))),
                StatusCode = StatusCode.BadRequest
            };
        }

        var user = new T { Email = email, NormalizedEmail = email.ToUpper() };
        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, registration.Password);

        if ( ! result.Succeeded ) {
            return new ServiceResponse { Data = CreateValidationProblem(result), StatusCode = StatusCode.InternalServerError};
        }

        await SendConfirmationEmailAsync(user, userManager, context, email);

        return new ServiceResponse { StatusCode = StatusCode.OK };
    }

    public async Task<ServiceResponse> Login(AuthLoginRequestDTO login, bool? useCookies, bool? useSessionCookies) {
        var useCookieScheme = true == useCookies || true == useSessionCookies;
        var isPersistent = true == useCookies && false == useSessionCookies;
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(login.Username, login.Password, isPersistent, lockoutOnFailure: true);

        if (result.RequiresTwoFactor) {
            if ( false == string.IsNullOrEmpty(login.TwoFactorCode)) {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
            }
            else if ( false == string.IsNullOrEmpty(login.TwoFactorRecoveryCode)) {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
            }
        }
        
        // todo: setup for JWT ? WHY ? Is this necessary ?? if it works it works. This is only used to manage user data and assets. Even User data for the most part will be stored on the machine

        return false == result.Succeeded
            ? new ServiceResponse {
                Data = TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized),
                StatusCode = StatusCode.Unauthorized
            }
            : new ServiceResponse { Data = TypedResults.Empty, StatusCode = StatusCode.OK };
    }
    // todo: Zombie code doesn't work. Can't be bothered to fix; will switch to identity. This is more effort than it's worth
    // public async Task<ServiceResponse> Refresh(RefreshRequest refreshRequest) {
    //     var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
    //     var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);
    //
    //     // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
    //     if (refreshTicket?.Properties.ExpiresUtc is not { } expiresUtc ||
    //         timeProvider.GetUtcNow() >= expiresUtc ||
    //         await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user) {
    //         return new ServiceResponse { Data = TypedResults.Challenge(), StatusCode = StatusCode.Unauthorized };
    //     }
    //
    //     var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
    //
    //     return new ServiceResponse {
    //         Data = TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme),
    //         StatusCode = StatusCode.OK
    //     };
    // }
    //
    // public async Task<ServiceResponse> ConfirmEmail(string userId, string code, string? changedEmail) {
    //     if (await userManager.FindByIdAsync(userId) is not { } user) {
    //         // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
    //         return new ServiceResponse { Data = TypedResults.Unauthorized(), StatusCode = StatusCode.Unauthorized };
    //     }
    //
    //     try {
    //         code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
    //     } 
    //     catch (FormatException) { 
    //         return new ServiceResponse { Data = TypedResults.Unauthorized(), StatusCode = StatusCode.Unauthorized };
    //     }
    //
    //     IdentityResult result;
    //
    //     if (string.IsNullOrEmpty(changedEmail)) {
    //         result = await userManager.ConfirmEmailAsync(user, code);
    //     }
    //     else {
    //         // As with Identity UI, email and user name are one and the same. So when we update the email,
    //         // we need to update the user name.
    //         result = await userManager.ChangeEmailAsync(user, changedEmail, code);
    //
    //         if (result.Succeeded) {
    //             result = await userManager.SetUserNameAsync(user, changedEmail);
    //         }
    //     }
    //
    //     return false == result.Succeeded ? 
    //         new ServiceResponse { Data = TypedResults.Unauthorized(), StatusCode = StatusCode.Unauthorized } : 
    //         new ServiceResponse { Message = "Thank you for confirming your email.", StatusCode = StatusCode.Unauthorized };
    // }
    //
    // public async Task<ServiceResponse> ResendConfirmationEmail(ResendConfirmationEmailRequest resendRequest, HttpContext context) {
    //     if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user) {
    //         return TypedResults.Ok();
    //     }
    //
    //     await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email);
    //     return TypedResults.Ok();
    // }
    //
    // public async Task<ServiceResponse> ForgotPassword(ForgotPasswordRequest resetRequest) {
    //     var user = await userManager.FindByEmailAsync(resetRequest.Email);
    //
    //     if (user is not null && await userManager.IsEmailConfirmedAsync(user))
    //     {
    //         var code = await userManager.GeneratePasswordResetTokenAsync(user);
    //         code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //
    //         await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
    //     }
    //
    //     // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
    //     // returned a 400 for an invalid code given a valid user email.
    //     return TypedResults.Ok();
    // }
    //
    // public async Task<ServiceResponse> ResetPassword(ResetPasswordRequest resetRequest) {
    //     var user = await userManager.FindByEmailAsync(resetRequest.Email);
    //
    //     if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
    //     {
    //         // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
    //         // returned a 400 for an invalid code given a valid user email.
    //         return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
    //     }
    //
    //     IdentityResult result;
    //     try {
    //         var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
    //         result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
    //     }
    //     catch (FormatException) {
    //         result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
    //     }
    //
    //     if (!result.Succeeded) {
    //         return CreateValidationProblem(result);
    //     }
    //
    //     return TypedResults.Ok();
    // }
    //
    // public async Task<ServiceResponse> Manage2FA(ClaimsPrincipal claimsPrincipal, TwoFactorRequest tfaRequest) {
    //     var userManager = signInManager.UserManager;
    //
    //     if (await userManager.GetUserAsync(claimsPrincipal) is not { } user) {
    //         return TypedResults.NotFound();
    //     }
    //
    //     if (tfaRequest.Enable == true) {
    //         if (tfaRequest.ResetSharedKey) {
    //             return CreateValidationProblem("CannotResetSharedKeyAndEnable",
    //                 "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
    //         }
    //         else if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode)) {
    //             return CreateValidationProblem("RequiresTwoFactor",
    //                 "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
    //         }
    //         else if (!await userManager.VerifyTwoFactorTokenAsync(user,
    //                      userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode)) {
    //             return CreateValidationProblem("InvalidTwoFactorCode",
    //                 "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
    //         }
    //
    //         await userManager.SetTwoFactorEnabledAsync(user, true);
    //     }
    //     else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey) {
    //         await userManager.SetTwoFactorEnabledAsync(user, false);
    //     }
    //
    //     if (tfaRequest.ResetSharedKey) {
    //         await userManager.ResetAuthenticatorKeyAsync(user);
    //     }
    //
    //     string[]? recoveryCodes = null;
    //
    //     if (tfaRequest.ResetRecoveryCodes ||
    //         (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0)) {
    //         var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
    //         recoveryCodes = recoveryCodesEnumerable?.ToArray();
    //     }
    //
    //     if (tfaRequest.ForgetMachine) {
    //         await signInManager.ForgetTwoFactorClientAsync();
    //     }
    //
    //     var key = await userManager.GetAuthenticatorKeyAsync(user);
    //
    //     if (string.IsNullOrEmpty(key)) {
    //         await userManager.ResetAuthenticatorKeyAsync(user);
    //         key = await userManager.GetAuthenticatorKeyAsync(user);
    //
    //         if (string.IsNullOrEmpty(key)) {
    //             throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
    //         }
    //     }
    //
    //     return TypedResults.Ok(new TwoFactorResponse {
    //         SharedKey = key,
    //         RecoveryCodes = recoveryCodes,
    //         RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
    //         IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
    //         IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
    //     });
    // }
    //
    // public async Task<ServiceResponse> ManageInfo(ClaimsPrincipal claimsPrincipal) {
    //     if (await userManager.GetUserAsync(claimsPrincipal) is not { } user) {
    //         return TypedResults.NotFound();
    //     }
    //
    //     return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    // }
    //
    // public async Task<ServiceResponse> ManageInfo(ClaimsPrincipal claimsPrincipal, InfoRequest infoRequest, HttpContext context) {
    //     if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)  {
    //         return TypedResults.NotFound();
    //     }
    //
    //     if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !_emailAddressAttribute.IsValid(infoRequest.NewEmail)) {
    //         return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
    //     }
    //
    //     if (!string.IsNullOrEmpty(infoRequest.NewPassword)) {
    //         if (string.IsNullOrEmpty(infoRequest.OldPassword)) {
    //             return CreateValidationProblem("OldPasswordRequired",
    //                 "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
    //         }
    //
    //         var changePasswordResult = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
    //         if (!changePasswordResult.Succeeded) {
    //             return CreateValidationProblem(changePasswordResult);
    //         }
    //     }
    //
    //     if (!string.IsNullOrEmpty(infoRequest.NewEmail)) {
    //         var email = await userManager.GetEmailAsync(user);
    //
    //         if (email != infoRequest.NewEmail) {
    //             await SendConfirmationEmailAsync(user, userManager, context, infoRequest.NewEmail, isChange: true);
    //         }
    //     }
    //
    //     return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    // }
    //
    // private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) {
    //     TypedResults.ValidationProblem(new Dictionary<string, string[]> {
    //         { errorCode, new[] { errorDescription } }
    //     });
    // }
    //
    private async Task SendConfirmationEmailAsync(T user, UserManager<T> userManager, HttpContext context, string email, bool isChange = false)
    {
        var code = isChange
            ? await userManager.GenerateChangeEmailTokenAsync(user, email)
            : await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    
        var userId = await userManager.GetUserIdAsync(user);
        var routeValues = new RouteValueDictionary() {
            ["userId"] = userId,
            ["code"] = code,
        };
    
        if (isChange) {
            // This is validated by the /confirmEmail endpoint on change.
            routeValues.Add("changedEmail", email);
        }
    
        // var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
        //                       ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");
        //
        // await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }
    
    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = new [] { error.Description } ;
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
    
    private static async Task<InfoResponse> CreateInfoResponseAsync<T>(T user, UserManager<T> userManager)
        where T : class
    {
        return new InfoResponse {
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata
    {
        public string? Name => null;
    }
}