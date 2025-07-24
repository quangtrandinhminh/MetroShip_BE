using FluentValidation;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Helpers;
using RestSharp.Extensions;

namespace MetroShip.Service.Validations;

public static class MetroLineValidator
{
    public static void ValidateMetroLineCreateRequest (this MetroLineCreateRequest request)
    {
        var validator = new MetroLineCreateValidator();
        validator.ValidateApiModel(request);
    }

    private static void ValidateCreateStationRequest(CreateStationRequest request)
    {
        var validator = new StationCreateValidator();
        validator.ValidateApiModel(request);
    }
}

public class MetroLineCreateValidator : AbstractValidator<MetroLineCreateRequest>
{
    public MetroLineCreateValidator()
    {
        RuleFor(x => x.LineNameVi)
            .NotEmpty().WithMessage("Line name in Vietnamese is required.")
            .MaximumLength(100).WithMessage("Line name in Vietnamese must not exceed 100 characters.");

        RuleFor(x => x.LineNameEn)
            .NotEmpty().WithMessage("Line name in English is required.")
            .MaximumLength(100).WithMessage("Line name in English must not exceed 100 characters.");

        RuleFor(x => x.RegionId)
            .NotEmpty().WithMessage("Region ID is required.")
            .Must(regionId => Guid.TryParse(regionId, out var _))
            .WithMessage("Region ID must be a valid GUID.");

        RuleFor(x => x.LineNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Line number must be greater than or equal to 0.");

        RuleFor(x => x.LineCode)
            .MaximumLength(20).WithMessage("Line code must not exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Line code must consist of letters and numbers only.")
            .When(x => !string.IsNullOrEmpty(x.LineCode));

        RuleFor(x => x.LineType)
            .MaximumLength(100).WithMessage("Line type must not exceed 100 characters.")
            .Matches(@"^[A-Za-z\s]+$").WithMessage("Line type must consist of letters and spaces only.")
            .When(x => !string.IsNullOrEmpty(x.LineType));

        RuleFor(x => x.LineOwner)
            .MaximumLength(100)
            .WithMessage("Line owner must not exceed 100 characters.")
            .Matches(@"^[A-Za-z\s]+$").WithMessage("Line owner must consist of letters and spaces only.")
            .When(x => !string.IsNullOrEmpty(x.LineOwner));

        RuleFor(x => x.ColorHex)
            .Matches(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")
            .WithMessage("Color hex must be a valid hex color code. #xxx #123456")
            .When(x => !string.IsNullOrEmpty(x.ColorHex));

        RuleFor(x => x.RouteTimeMin)
            .GreaterThanOrEqualTo(0).WithMessage("Route time must be >= 0")
            .When(x => x.RouteTimeMin.HasValue);

        RuleFor(x => x.DwellTimeMin)
            .GreaterThanOrEqualTo(0).WithMessage("Dwell time must be >= 0")
            .When(x => x.DwellTimeMin.HasValue);

        RuleFor(x => x.Stations)
            .NotEmpty().WithMessage("At least one station is required.")
            .Must(stations => stations.Count >= 2)
            .WithMessage("At least two stations are required for a metro line.");

        RuleForEach(x => x.Stations).SetValidator(new StationCreateValidator());
    }
}

public class StationCreateValidator : AbstractValidator<CreateStationRequest>
{
    public StationCreateValidator()
    {
        // If Id is provided, it must be a valid GUID (but can be null)
        RuleFor(x => x.Id)
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out var _))
            .WithMessage("Station ID must be a valid GUID when provided.");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive must be specified when creating new station.");

        RuleFor(x => x.ToNextStationKm)
            .NotNull().WithMessage("ToNextStationKm is required when creating new station.")
            .GreaterThanOrEqualTo(0).WithMessage("ToNextStationKm must be non-negative.");

        // When Id is null/empty, all other fields are required
        When(x => string.IsNullOrEmpty(x.Id), () => {
            RuleFor(x => x.StationNameVi)
                .NotEmpty().WithMessage("Station name in Vietnamese is required when creating new station.")
                .MaximumLength(100).WithMessage("Station name in Vietnamese must not exceed 100 characters.");

            RuleFor(x => x.StationNameEn)
                .NotEmpty().WithMessage("Station name in English is required when creating new station.")
                .MaximumLength(100).WithMessage("Station name in English must not exceed 100 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required when creating new station.")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

            RuleFor(x => x.IsUnderground)
                .NotNull().WithMessage("IsUnderground must be specified when creating new station.");

            RuleFor(x => x.RegionId)
                .NotEmpty().WithMessage("Region ID is required when creating new station.")
                .Must(regionId => Guid.TryParse(regionId, out var _))
                .WithMessage("Region ID must be a valid GUID when creating new station.");

            RuleFor(x => x.Latitude)
                .NotNull().WithMessage("Latitude is required when creating new station.")
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .NotNull().WithMessage("Longitude is required when creating new station.")
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");
        });

        // When Id is provided, other fields should be null/empty (optional validation)
        When(x => !string.IsNullOrEmpty(x.Id), () => {
            RuleFor(x => x.StationNameVi)
                .Empty().WithMessage("Station name should not be provided when using existing station ID.")
                .When(x => !string.IsNullOrEmpty(x.StationNameVi));

            // Add similar rules for other fields if you want strict validation
        });
    }
}