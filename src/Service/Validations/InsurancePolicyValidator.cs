using FluentValidation;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.InsurancePolicy;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;

namespace MetroShip.Service.Validations;

public static class InsurancePolicyValidator
{
    public static void ValidatePricingConfigRequest(InsurancePolicyRequest request)
    {
        var validator = new InsurancePolicyRequestValidator();
        validator.ValidateApiModel(request);
    }
}

public class InsurancePolicyRequestValidator : AbstractValidator<InsurancePolicyRequest>
{
    public InsurancePolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ResponseMessageInsurancePolicy.NAME_REQUIRED)
            .MaximumLength(255)
            .WithMessage("Tên chính sách bảo hiểm không được vượt quá 255 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Mô tả không được vượt quá 1000 ký tự.");

        RuleFor(x => x.BaseFeeVnd)
            .NotNull()
            .WithMessage("Phí cơ bản không được để trống.")
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageInsurancePolicy.BASE_FEE_VND_INVALID);

        RuleFor(x => x.MaxParcelValueVnd)
            .NotNull()
            .WithMessage("Giá trị hàng hóa tối đa được bảo hiểm không được để trống.")
            .GreaterThan(0)
            .WithMessage(ResponseMessageInsurancePolicy.MAX_PARCEL_VALUE_VND_INVALID);

        RuleFor(x => x.InsuranceFeeRateOnValue)
            .NotNull()
            .WithMessage("Tỷ lệ phí bảo hiểm không được để trống.")
            .InclusiveBetween(0m, 1m)
            .WithMessage(ResponseMessageInsurancePolicy.INSURANCE_FEE_RATE_ON_VALUE_INVALID);

        RuleFor(x => x.MaxCompensationRateOnValue)
            .NotNull()
            .WithMessage("Tỷ lệ bồi thường tối đa theo giá trị hàng hóa không được để trống.")
            .InclusiveBetween(0m, 1m)
            .WithMessage(ResponseMessageInsurancePolicy.MAX_COMPENSATION_RATE_ON_VALUE_INVALID);

        RuleFor(x => x.MinCompensationRateOnValue)
            .NotNull()
            .WithMessage("Tỷ lệ bồi thường tối thiểu theo giá trị hàng hóa không được để trống.")
            .InclusiveBetween(0m, 1m)
            .WithMessage(ResponseMessageInsurancePolicy.MIN_COMPENSATION_RATE_ON_VALUE_INVALID)
            .LessThanOrEqualTo(x => x.MaxCompensationRateOnValue)
            .WithMessage("Tỷ lệ bồi thường tối thiểu phải nhỏ hơn hoặc bằng tỷ lệ bồi thường tối đa.");

        RuleFor(x => x.MinCompensationRateOnShippingFee)
            .NotNull()
            .WithMessage("Tỷ lệ bồi thường tối đa theo phí vận chuyển không được để trống.")
            .GreaterThanOrEqualTo(4)
            .WithMessage(ResponseMessageInsurancePolicy.MIN_COMPENSATION_RATE_ON_SHIPPING_FEE_INVALID);
    }
}