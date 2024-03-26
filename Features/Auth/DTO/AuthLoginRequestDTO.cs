namespace RESTful_web_API_Course.Features.Auth.Models; 

/// <summary>
/// A custom implementation of the .Net identity login model.
/// This changes email for username which is more ambiguous towards choice. It can reference both email and username
/// depending on the context of the login.
///
/// For example before the user selects a username they will create an account with an email address.
/// Once created they will be greeted with a way to create a username. This will then be their login username once set.
///
/// This is done to help capture users details in case they fail to complete their registration process
/// </summary>
public class AuthLoginRequestDTO { //todo: I'd rather just use Identity 🤷‍♂️ what's the point of this
    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// The user's password.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// The optional two-factor authenticator code. This may be required for users who have enabled two-factor authentication.
    /// This is not required if a <see cref="TwoFactorRecoveryCode"/> is sent.
    /// </summary>
    public string? TwoFactorCode { get; init; }

    /// <summary>
    /// An optional two-factor recovery code from <see cref="TwoFactorResponse.RecoveryCodes"/>.
    /// This is required for users who have enabled two-factor authentication but lost access to their <see cref="TwoFactorCode"/>.
    /// </summary>
    public string? TwoFactorRecoveryCode { get; init; }
}