using FluentValidation;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Helpers;

namespace MetroShip.Service.Validations;

public static class MetroLineValidator
{
    public static void ValidateMetroLineCreateRequest (MetroLineCreateRequest request)
    {
        var validator = new MetroLineCreateValidator();
        validator.ValidateApiModel(request);

        foreach (var station in request.Stations)
        {
            ValidateCreateStationRequest(station);
        }
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

        RuleFor(x => x.Stations)
            .NotEmpty().WithMessage("At least one station is required.")
            .Must(stations => stations.Count >= 2)
            .WithMessage("At least two stations are required for a metro line.");
    }
}

public class StationCreateValidator : AbstractValidator<CreateStationRequest>
{
    public StationCreateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Station ID is required.")
            .Must(id => Guid.TryParse(id, out var _))
            .WithMessage("Station ID must be a valid GUID.")
            .When(x => x != null);

        RuleFor(x => x.StationNameVi)
            .NotEmpty().WithMessage("Station name in Vietnamese is required.")
            .MaximumLength(100).WithMessage("Station name in Vietnamese must not exceed 100 characters.")
            .When(x => x.Id == null && x.StationNameVi != null);

        RuleFor(x => x.StationNameEn)
            .NotEmpty().WithMessage("Station name in English is required.")
            .MaximumLength(100).WithMessage("Station name in English must not exceed 100 characters.")
            .When(x => x.Id == null && x.StationNameEn != null);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.")
            .When(x => x.Id == null && x.Address != null);

        RuleFor(x => x.IsUnderground)
            .NotNull().WithMessage("IsUnderground must be specified.")
            .Must(isUnderground => isUnderground == true || isUnderground == false)
            .When(x => x.Id == null && x.IsUnderground.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive must be specified.")
            .Must(isActive => isActive == true || isActive == false)
            .When(x => x.Id == null && x.IsActive.HasValue);

        RuleFor(x => x.RegionId)
            .NotEmpty().WithMessage("Region ID is required.")
            .Must(regionId => Guid.TryParse(regionId, out var _))
            .WithMessage("Region ID must be a valid GUID.")
            .When(x => x.Id == null && x.RegionId != null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.")
            .When(x => x.Id == null && x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.")
            .When(x => x.Id == null && x.Longitude.HasValue);

    }
}