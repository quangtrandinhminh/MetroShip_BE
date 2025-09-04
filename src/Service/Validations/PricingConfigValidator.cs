using FluentValidation;
using MetroShip.Service.ApiModels;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.Helpers;

namespace MetroShip.Service.Validations;

public static class PricingConfigValidator
{
    public static void ValidatePricingConfigRequest(PricingConfigRequest request)
    {
        var validator = new PricingConfigRequestValidator();
        validator.ValidateApiModel(request);
    }
}

public class PricingConfigRequestValidator : AbstractValidator<PricingConfigRequest>
{
    public PricingConfigRequestValidator()
    {
        RuleFor(x => x.WeightTiers)
            .NotNull()
            .WithMessage("Danh sách tier trọng lượng không được null.")
            .NotEmpty()
            .WithMessage("Phải có ít nhất một tier trọng lượng.");

        RuleFor(x => x.DistanceTiers)
            .NotNull()
            .WithMessage("Danh sách tier khoảng cách không được null.")
            .NotEmpty()
            .WithMessage("Phải có ít nhất một tier khoảng cách.");

        RuleFor(x => x.WeightTiers)
            .Must(ValidateWeightTiersOrder)
            .When(x => x.WeightTiers != null && x.WeightTiers.Any())
            .WithMessage("Các tier trọng lượng phải được sắp xếp theo thứ tự tăng dần không chồng lấp.");

        RuleFor(x => x.DistanceTiers)
            .Must(ValidateDistanceTiersOrder)
            .When(x => x.DistanceTiers != null && x.DistanceTiers.Any())
            .WithMessage("Các tier khoảng cách phải được sắp xếp theo thứ tự tăng dần không chồng lấp.");

        RuleForEach(x => x.WeightTiers)
            .SetValidator(new WeightTierRequestValidator());

        RuleForEach(x => x.DistanceTiers)
            .SetValidator(new DistanceTierRequestValidator());

        RuleFor(x => x.FreeStoreDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FreeStoreDays.HasValue)
            .WithMessage("Số ngày lưu kho miễn phí phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.BaseSurchargePerDayVnd)
            .GreaterThan(0)
            .When(x => x.BaseSurchargePerDayVnd.HasValue)
            .WithMessage("Phí phụ thu cơ bản mỗi ngày phải lớn hơn 0.");

        RuleFor(x => x.RefundRate)
            .InclusiveBetween(0m, 1m)
            .When(x => x.RefundRate.HasValue)
            .WithMessage("Tỷ lệ hoàn tiền phải từ 0 đến 1.");

        RuleFor(x => x.RefundForCancellationBeforeScheduledHours)
            .GreaterThanOrEqualTo(0)
            .When(x => x.RefundForCancellationBeforeScheduledHours.HasValue)
            .WithMessage("Số giờ hoàn tiền trước khi hủy phải lớn hơn hoặc bằng 0.");
    }

    private bool ValidateWeightTiersOrder(IList<WeightTierRequest> weightTiers)
    {
        if (weightTiers == null || !weightTiers.Any())
            return true;

        var sortedTiers = weightTiers.OrderBy(t => t.TierOrder).ToList();
        decimal previousMaxWeight = 0;

        foreach (var tier in sortedTiers)
        {
            if (tier.MaxWeightKg == null || tier.MaxWeightKg <= 0)
                return false;

            if (tier.MaxWeightKg <= previousMaxWeight)
                return false;

            previousMaxWeight = tier.MaxWeightKg.Value;
        }

        return true;
    }

    private bool ValidateDistanceTiersOrder(IList<DistanceTierRequest> distanceTiers)
    {
        if (distanceTiers == null || !distanceTiers.Any())
            return true;

        var sortedTiers = distanceTiers.OrderBy(t => t.TierOrder).ToList();
        decimal previousMaxDistance = 0;

        foreach (var tier in sortedTiers)
        {
            if (tier.MaxDistanceKm == null || tier.MaxDistanceKm <= 0)
                return false;

            if (tier.MaxDistanceKm <= previousMaxDistance)
                return false;

            previousMaxDistance = tier.MaxDistanceKm.Value;
        }

        return true;
    }
}

public class WeightTierRequestValidator : AbstractValidator<WeightTierRequest>
{
    public WeightTierRequestValidator()
    {
        RuleFor(x => x.TierOrder)
            .GreaterThan(0)
            .WithMessage("Thứ tự tier phải lớn hơn 0.");

        RuleFor(x => x.MaxWeightKg)
            .NotNull()
            .WithMessage("Trọng lượng tối đa không được null.")
            .GreaterThan(0)
            .WithMessage("Trọng lượng tối đa phải lớn hơn 0.");

        RuleFor(x => x.BasePriceVnd)
            .NotNull()
            .WithMessage("Giá cơ bản không được null.")
            .GreaterThan(0)
            .WithMessage("Giá cơ bản phải lớn hơn 0.")
            .When(x => !x.IsPricePerKmAndKg);

        RuleFor(x => x.BasePriceVndPerKmPerKg)
            .NotNull()
            .WithMessage("Giá cơ bản theo km và kg không được null.")
            .GreaterThan(0)
            .WithMessage("Giá cơ bản theo km và kg phải lớn hơn 0.")
            .When(x => x.IsPricePerKmAndKg);
    }
}

public class DistanceTierRequestValidator : AbstractValidator<DistanceTierRequest>
{
    public DistanceTierRequestValidator()
    {
        RuleFor(x => x.TierOrder)
            .GreaterThan(0)
            .WithMessage("Thứ tự tier phải lớn hơn 0.");

        RuleFor(x => x.MaxDistanceKm)
            .NotNull()
            .WithMessage("Khoảng cách tối đa không được null.")
            .GreaterThan(0)
            .WithMessage("Khoảng cách tối đa phải lớn hơn 0.");

        RuleFor(x => x.BasePriceVnd)
            .NotNull()
            .WithMessage("Giá cơ bản không được null.")
            .GreaterThan(0)
            .WithMessage("Giá cơ bản phải lớn hơn 0.")
            .When(x => !x.IsPricePerKm);

        RuleFor(x => x.BasePriceVndPerKm)
            .NotNull()
            .WithMessage("Giá cơ bản theo km không được null.")
            .GreaterThan(0)
            .WithMessage("Giá cơ bản theo km phải lớn hơn 0.")
            .When(x => x.IsPricePerKm);
    }
}