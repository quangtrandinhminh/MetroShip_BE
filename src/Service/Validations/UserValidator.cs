using FluentValidation;
using MetroShip.Service.ApiModels.User;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.Validations;

public class UserValidator
{
    private readonly IValidator<UserCreateRequest> _userRequestValidator;
    private readonly IValidator<UserUpdateRequest> _userUpdateRequestValidator;

    public UserValidator()
    {
        _userRequestValidator = new UserCreateRequestValidator();
        _userUpdateRequestValidator = new UserUpdateRequestValidator();
    }

    public void ValidateUserCreateRequest(UserCreateRequest request)
    {
        _userRequestValidator.ValidateApiModel(request);
    }

    public void ValidateUserUpdateRequest(UserUpdateRequest request)
    {
        _userUpdateRequestValidator.ValidateApiModel(request);
    }
}

public sealed class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.USERNAME_REQUIRED)
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.NAME_REQUIRED)
            .MaximumLength(100)
            .Matches("^[^0-9]+$").WithMessage(ResponseMessageIdentity.NAME_INVALID);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResponseMessageIdentity.EMAIL_REQUIRED)
            .EmailAddress().WithMessage(ResponseMessageIdentity.EMAIL_INVALID);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(ResponseMessageIdentity.PHONENUMBER_REQUIRED)
            .Matches(@"^\d{10}$").WithMessage(ResponseMessageIdentity.PHONENUMBER_INVALID)
            .Length(10).WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResponseMessageIdentity.PASSWORD_REQUIRED)
            .MinimumLength(8).WithMessage(ResponseMessageIdentity.PASSSWORD_LENGTH)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(ResponseMessageIdentity.CONFIRM_PASSWORD_REQUIRED)
            .Equal(x => x.Password).WithMessage(ResponseMessageIdentity.PASSWORD_NOT_MATCH)
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage(ResponseMessageIdentity.ROLES_REQUIRED);

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTimeOffset.Now).WithMessage(ResponseMessageIdentity.BIRTHDATE_INVALID)
            .When(x => x.BirthDate.HasValue);
    }
}

public sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.UserName)
            .MaximumLength(100)
            // Allow alphanumeric characters, number,no spaces, and special characters like . _ -
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage(ResponseMessageIdentity.USERNAME_INVALID)
            .When(x => !string.IsNullOrEmpty(x.UserName));

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            // Allow only alphabetic characters, no numbers
            .Matches("^[^0-9]+$").WithMessage(ResponseMessageIdentity.NAME_INVALID)
            .When(x => !string.IsNullOrEmpty(x.FullName)); 

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Avatar)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Avatar));

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTimeOffset.Now).WithMessage(ResponseMessageIdentity.BIRTHDATE_INVALID)
            .When(x => x.BirthDate.HasValue);
    }
}