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
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage(ResponseMessageIdentity.USER_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _)).WithMessage(ResponseMessageIdentity.USER_ID_INVALID);


        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.USERNAME_REQUIRED)
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(ResponseMessageIdentity.NAME_REQUIRED)
            .MaximumLength(100)
            .Matches("^[^0-9]+$").WithMessage(ResponseMessageIdentity.NAME_INVALID);

        RuleFor(x => x.Address)
            .MaximumLength(200);

        RuleFor(x => x.Avatar)
            .MaximumLength(500);

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTimeOffset.Now).WithMessage(ResponseMessageIdentity.BIRTHDATE_INVALID)
            .When(x => x.BirthDate.HasValue);
    }
}