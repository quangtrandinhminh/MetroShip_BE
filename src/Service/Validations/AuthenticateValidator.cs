using FluentValidation;
using MetroShip.Repository.Models.Identity;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using MetroShip.Utility.Exceptions;

namespace MetroShip.Service.Validations;

public sealed class AuthValidator
{
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;
    private readonly IValidator<PhoneLoginRequest> _phoneLoginRequestValidator;
    private readonly UserEntityValidator _entityValidator;

    public AuthValidator()
    {
        _loginRequestValidator = new LoginRequestValidator();
        _entityValidator = new UserEntityValidator();
        _registerRequestValidator = new RegisterRequestValidator();
        _phoneLoginRequestValidator = new PhoneLoginRequestValidator();
    }

    public void ValidateLoginRequest(LoginRequest request)
    {
        _loginRequestValidator.ValidateApiModel(request);
    }

    public void ValidateRegisterRequest(RegisterRequest request)
    {
        _registerRequestValidator.ValidateApiModel(request);
    }

    public void ValidateLogin(LoginRequest dto, UserEntity? account)
    {
        // Validate Entity
        if (account == null)
        {
            throw new AppException(ErrorCode.UserInvalid, 
                ResponseMessageIdentity.INVALID_USER, StatusCodes.Status401Unauthorized);
        }

        var entity = (account, dto.Password);
        _entityValidator.ValidateBusiness(entity, StatusCodes.Status401Unauthorized);
    }

    public void ValidatePhoneLogin (PhoneLoginRequest request)
    {
        _phoneLoginRequestValidator.ValidateApiModel(request);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().MinimumLength(4)
            .WithMessage($"{nameof(LoginRequest.Username)} is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage($"{nameof(LoginRequest.Password)} is required");
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // UserName validation
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.USERNAME_REQUIRED)
            .MaximumLength(100);

        // FullName validation
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.NAME_REQUIRED)
            .MaximumLength(100)
            .Matches("^[^0-9]+$").WithMessage("Name cannot contain number");

        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResponseMessageIdentity.EMAIL_REQUIRED)
            .EmailAddress().WithMessage("Invalid email format");

        // PhoneNumber validation
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(ResponseMessageIdentity.PHONENUMBER_REQUIRED)
            .Matches(@"^\d{10}$").WithMessage(ResponseMessageIdentity.PHONENUMBER_INVALID)
            .Length(10).WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResponseMessageIdentity.PASSWORD_REQUIRED)
            .MinimumLength(8).WithMessage(ResponseMessageIdentity.PASSSWORD_LENGTH)
            .MaximumLength(100);

        // ConfirmPassword validation
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(ResponseMessageIdentity.CONFIRM_PASSWORD_REQUIRED)
            .Equal(x => x.Password).WithMessage(ResponseMessageIdentity.PASSWORD_NOT_MATCH)
            .MaximumLength(100);

        // Address validation (optional, no rules needed if nullable)
        RuleFor(x => x.Address)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address)); // Optional: Add a max length if needed

        // Avatar validation (optional, no rules needed if nullable)
        RuleFor(x => x.Avatar)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Avatar)); // Optional: Add a max length if needed

        // BirthDate validation (optional, no rules needed if nullable)
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now)).WithMessage("BirthDate cannot be in the future")
            .When(x => x.BirthDate.HasValue);
    }
}


// Validate UserEntity Account
public sealed class UserEntityValidator : AbstractValidator<(UserEntity Account, string Password)>
{
    public UserEntityValidator()
    {
        RuleFor(x => x.Account.DeletedTime)
            .Null().WithMessage(ResponseMessageIdentity.INVALID_USER).WithErrorCode(ErrorCode.UserInvalid);
        RuleFor(x => x.Account.IsActive).Equal(true)
            .WithMessage(ResponseMessageIdentity.EMAIL_VALIDATION_REQUIRED).WithErrorCode(ErrorCode.UserInActive);
        RuleFor(x => x)
            .Must(x => BCrypt.Net.BCrypt.Verify(x.Password, x.Account.PasswordHash))
            .WithMessage(ResponseMessageIdentity.PASSWORD_WRONG).WithErrorCode(ErrorCode.UserPasswordWrong);
    }
}

public sealed class PhoneLoginRequestValidator : AbstractValidator<PhoneLoginRequest>
{
    public PhoneLoginRequestValidator()
    {
        // PhoneNumber validation E.164 format
        var expression = @"^\+?[0-9]{10,15}$";

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(ResponseMessageIdentity.PHONENUMBER_REQUIRED)
            .Matches(expression).WithMessage(ResponseMessageIdentity.PHONENUMBER_INVALID)
            .Length(10).WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);
    }
}
