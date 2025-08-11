using FluentValidation;
using FluentValidation.Results;
using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using RestSharp.Extensions;

namespace MetroShip.Service.Validations;

public static class ShipmentValidator
{
    /*private readonly IValidator<ShipmentRequest> _shipmentRequestValidator;
    private readonly IValidator<ShipmentItineraryRequest> _itineraryRequestValidator;
    private readonly IValidator<ParcelRequest> _parcelRequestValidator;
    private readonly IValidator<TotalPriceCalcRequest> _totalPriceCalcRequestValidator;
    private readonly IValidator<ShipmentFilterRequest> _shipmentFilterRequestValidator;
    public ShipmentValidator()
    {
        _shipmentRequestValidator = new ShipmentRequestValidator();
        _itineraryRequestValidator = new ShipmentItineraryRequestValidator();
        _parcelRequestValidator = new ParcelRequestValidator();
        _totalPriceCalcRequestValidator = new TotalPriceCalcRequestValidator();
        _shipmentFilterRequestValidator = new ShipmentFilterRequestValidator();
    }*/

    public static void ValidateShipmentRequest(ShipmentRequest request)
    {
        var _shipmentRequestValidator = new ShipmentRequestValidator();
        _shipmentRequestValidator.ValidateApiModel(request);
    }

    public static void ValidateTotalPriceCalcRequest(TotalPriceCalcRequest request)
    {
        var _totalPriceCalcRequestValidator = new TotalPriceCalcRequestValidator();
        _totalPriceCalcRequestValidator.ValidateApiModel(request);
    }

    public static void ValidateShipmentFilterRequest(ShipmentFilterRequest request)
    {
        var _shipmentFilterRequestValidator = new ShipmentFilterRequestValidator();
        _shipmentFilterRequestValidator.ValidateApiModel(request);
    }

    public static void ValidateShipmentFeedbackRequest(ShipmentFeedbackRequest request)
    {
        var _shipmentFeedbackRequestValidator = new ShipmentFeedbackRequestValidator();
        _shipmentFeedbackRequestValidator.ValidateApiModel(request);
    }

    public static void ValidateChargeableWeightRequest(ChargeableWeightRequest request)
    {
        var _chargeableWeightRequestValidator = new ChargeableWeightRequestValidator();
        _chargeableWeightRequestValidator.ValidateApiModel(request);
    }
}

public class ShipmentRequestValidator : AbstractValidator<ShipmentRequest>
{
    public ShipmentRequestValidator()
    {
        RuleFor(x => x.StartReceiveAt)
            .LessThan(x => x.ScheduledDateTime)
            .WithMessage(ResponseMessageShipment.START_RECEIVE_AT_INVALID)
            .When(x => x.StartReceiveAt.HasValue);

        RuleFor(x => x.ScheduledDateTime)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPMENT_DATE_REQUIRED);

        RuleFor(x => x.TimeSlotId)
            .NotEmpty()

            .Must(id => string.IsNullOrWhiteSpace(id) || Guid.TryParse(id, out _))
            .WithMessage("TimeSlotId must be a valid GUID.");

        RuleFor(x => x.DepartureStationId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.DEPARTURE_STATION_ID_REQUIRED);

        RuleFor(x => x.SenderName)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SENDER_NAME_REQUIRED);

        RuleFor(x => x.SenderPhone)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SENDER_PHONE_REQUIRED)
            .Matches(@"^\d{10,15}$")
            .WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);

        RuleFor(x => x.RecipientName)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.RECIPIENT_NAME_REQUIRED);

        RuleFor(x => x.RecipientPhone)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.RECIPIENT_PHONE_REQUIRED)
            .Matches(@"^\d{10,15}$")
            .WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);

        RuleFor(x => x.RecipientEmail)
            .EmailAddress()
            .WithMessage("Recipient " + ResponseMessageIdentity.EMAIL_INVALID)
            .When(x => !string.IsNullOrEmpty( x.RecipientEmail));

        /*RuleFor(x => x.RecipientNationalId)
            // .NotEmpty()
            // .WithMessage(ResponseMessageShipment.RECIPIENT_NATIONAL_ID_REQUIRED)
            .Matches(@"^\d{9,12}$")
            .WithMessage(ResponseMessageShipment.RECIPIENT_NATIONAL_ID_INAVLID)
            .When(x => !string.IsNullOrEmpty(x.RecipientNationalId));*/

        RuleFor(x => x.TotalCostVnd)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_REQUIRED)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_INVALID);

        RuleFor(x => x.TotalInsuranceFeeVnd)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageShipment.INSURANCE_FEE_VND_INVALID)
            .When(x => x.TotalInsuranceFeeVnd.HasValue);

        RuleFor(x => x.TotalShippingFeeVnd)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPPING_FEE_VND_REQUIRED)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageShipment.SHIPPING_FEE_VND_INVALID);

        RuleFor(x => x.Parcels)
            .NotEmpty()
            .WithMessage(ResponseMessageParcel.PARCEL_REQUIRED)
            .Must(x => x.Count > 0)
            .WithMessage(ResponseMessageParcel.PARCEL_REQUIRED);

        RuleFor(x => x.ShipmentItineraries)
            .NotEmpty()
            .WithMessage(ResponseMessageItinerary.ITINERARY_REQUIRED)
            .Must(x => x.Count > 0)
            .WithMessage(ResponseMessageItinerary.ITINERARY_REQUIRED);

        RuleFor(x => x.TotalKm)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.TOTAL_KM_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageShipment.TOTAL_KM_INVALID);

        // for each itinerary, validate the route ID and leg order
        RuleForEach(x => x.ShipmentItineraries)
            .SetValidator(new ShipmentItineraryRequestValidator());

        // for each parcel, validate the parcel category ID and dimensions
        RuleForEach(x => x.Parcels)
            .SetValidator(new ParcelRequestValidator());
    }

    private static bool BeAValidShipmentStatus(ShipmentStatusEnum? status)
    {
        if (!status.HasValue) return true;

        return Enum.IsDefined(typeof(ShipmentStatusEnum), status.Value);
    }

    private static bool BeAValidShift(ShiftEnum? shift)
    {
        if (!shift.HasValue) return true;

        return Enum.IsDefined(typeof(ShiftEnum), shift.Value);
    }
}

public class ShipmentItineraryRequestValidator : AbstractValidator<ShipmentItineraryRequest>
{
    public ShipmentItineraryRequestValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageItinerary.ROUTE_ID_REQUIRED);

        RuleFor(x => x.LegOrder)
            .NotEmpty()
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_REQUIRED)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_INVALID);
    }
}

public class ParcelRequestValidator : AbstractValidator<ParcelRequest>
{
    public ParcelRequestValidator()
    {
        RuleFor(x => x.ParcelCategoryId)
            .NotNull()
            .WithMessage(ResponseMessageParcel.PARCEL_CATEGORY_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageParcel.PARCEL_CATEGORY_ID_INVALID);

        RuleFor(x => x.CategoryInsuranceId)
            .NotNull()
            .WithMessage(ResponseMessageParcel.CATEGORY_INSURANCE_ID_REQUIRED)
            .Must(x => string.IsNullOrWhiteSpace(x) || Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageParcel.CATEGORY_INSURANCE_ID_INVALID);

        RuleFor(x => x.WidthCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.WIDTH_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.WIDTH_INVALID);

        RuleFor(x => x.HeightCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.HEIGHT_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.HEIGHT_INVALID);

        RuleFor(x => x.LengthCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.LENGTH_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.LENGTH_INVALID);

        RuleFor(x => x.WeightKg)
            .NotNull()
            .WithMessage(ResponseMessageParcel.WEIGHT_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.WEIGHT_INVALID);

        RuleFor(x => x.ChargeableWeight)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.CHARGEABLE_WEIGHT_INVALID)
            .When(x => x.ChargeableWeight.HasValue);

        RuleFor(x => x.ShippingFeeVnd)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageParcel.SHIPPING_FEE_VND_INVALID)
            .When(x => x.ShippingFeeVnd.HasValue);

        RuleFor(x => x.InsuranceFeeVnd)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageParcel.INSURANCE_FEE_VND_INVALID)
            .When(x => x.InsuranceFeeVnd.HasValue);

        RuleFor(x => x.PriceVnd)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageParcel.PRICE_VND_INVALID)
            .When(x => x.PriceVnd.HasValue);

        RuleFor(x => x.ValueVnd)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.VALUE_VND_INVALID)
            .When(x => x.ValueVnd.HasValue);

        RuleFor(x => x.IsBulk)
            .Must(x => x == true || x == false)
            .When(x => x.IsBulk.HasValue)
            .WithMessage(ResponseMessageParcel.IS_BULK_INVALID);
    }
}

public class TotalPriceCalcRequestValidator : AbstractValidator<TotalPriceCalcRequest>
{
    public TotalPriceCalcRequestValidator()
    {
        RuleFor(x => x.DepartureStationId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.DEPARTURE_STATION_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageShipment.DEPARTURE_STATION_ID_INVALID);

        RuleFor(x => x.DestinationStationId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.DESTINATION_STATION_ID_REQUIRED)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageShipment.DESTINATION_STATION_ID_INVALID);

        RuleFor(x => x.ScheduledDateTime)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPMENT_DATE_REQUIRED);

        RuleFor(x => x.TimeSlotId)
            .NotEmpty()
            .WithMessage("TimeSlotId is required.")
            .Must(id => string.IsNullOrWhiteSpace(id) || Guid.TryParse(id, out _))
            .WithMessage("TimeSlotId must be a valid GUID.");

        RuleFor(x => x.Parcels)
            .NotEmpty()
            .WithMessage(ResponseMessageParcel.PARCEL_REQUIRED);

        RuleForEach(x => x.Parcels)
            .SetValidator(new ParcelRequestValidator());
    }
}

public class ShipmentFilterRequestValidator : AbstractValidator<ShipmentFilterRequest>
{
    public ShipmentFilterRequestValidator()
    {
        // Validate ShipmentStatus - check if it's a valid enum value
        RuleFor(x => x.ShipmentStatus)
            .Must(BeAValidShipmentStatus)
            .WithMessage(ResponseMessageShipment.SHIPMENT_STATUS_INVALID)
            .When(x => x.ShipmentStatus.HasValue);

        // Validate TrackingCode format if needed
        RuleFor(x => x.TrackingCode)
            .MaximumLength(50)
            .WithMessage("Tracking code cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TrackingCode));

        // Validate date range
        RuleFor(x => x.FromScheduleDateTime)
            .LessThanOrEqualTo(x => x.ToScheduleDateTime)
            .WithMessage("From date must be earlier than or equal to To date")
            .When(x => x.FromScheduleDateTime.HasValue && x.ToScheduleDateTime.HasValue);

        // Validate date range is not too far in the past or future
        RuleFor(x => x.FromScheduleDateTime)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.AddYears(-5))
            .WithMessage("From date cannot be more than 5 years in the past")
            .When(x => x.FromScheduleDateTime.HasValue);

        RuleFor(x => x.ToScheduleDateTime)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddYears(1))
            .WithMessage("To date cannot be more than 1 year in the future")
            .When(x => x.ToScheduleDateTime.HasValue);

        // Validate phone number format
        RuleFor(x => x.SenderPhone)
            .Matches(@"^[\+]?[0-9\-\(\)\s]+$")
            .WithMessage("Invalid phone number format")
            .Length(10, 15)
            .WithMessage("Phone number must be between 10 and 15 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SenderPhone));

        RuleFor(x => x.RecipientPhone)
            .Matches(@"^[\+]?[0-9\-\(\)\s]+$")
            .WithMessage("Invalid phone number format")
            .Length(10, 15)
            .WithMessage("Phone number must be between 10 and 15 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientPhone));

        // Validate email format
        RuleFor(x => x.RecipientEmail)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientEmail));

        // Validate TimeSlotId format
        RuleFor(x => x.TimeSlotId)
            .Must(id => string.IsNullOrWhiteSpace(id) || Guid.TryParse(id, out _))
            .WithMessage("TimeSlotId must be a valid GUID.");

        // Validate string length constraints
        RuleFor(x => x.DepartureStationId)
            .Must(x => string.IsNullOrWhiteSpace(x) || Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageShipment.DEPARTURE_STATION_ID_INVALID);

        RuleFor(x => x.DestinationStationId)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageShipment.DESTINATION_STATION_ID_INVALID)
            .When(x => !string.IsNullOrWhiteSpace(x.DestinationStationId));

        RuleFor(x => x.SenderId)
            .Must(x => string.IsNullOrWhiteSpace(x) || Guid.TryParse(x, out _))
            .WithMessage("Sender ID must be a valid GUID.")
            .When(x => !string.IsNullOrWhiteSpace(x.SenderId));

        RuleFor(x => x.SenderName)
            .MaximumLength(100)
            .WithMessage("Sender name cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SenderName));

        RuleFor(x => x.RecipientName)
            .MaximumLength(100)
            .WithMessage("Recipient name cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientName));

        RuleFor(x => x.RegionCode)
            .MaximumLength(10)
            .WithMessage("Region code cannot exceed 10 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RegionCode));

        RuleFor(x => x.LineId)
            .Must(x => string.IsNullOrWhiteSpace(x) || Guid.TryParse(x, out _))
            .WithMessage("Line ID must be a valid GUID");

        RuleFor(x => x.ItineraryIncludeStationId)
            .Must(x => string.IsNullOrWhiteSpace(x) || Guid.TryParse(x, out _))
            .WithMessage("Itinerary include station ID must be a valid GUID");
    }

    private static bool BeAValidShipmentStatus(ShipmentStatusEnum? status)
    {
        if (!status.HasValue) return true;

        return Enum.IsDefined(typeof(ShipmentStatusEnum), status.Value);
    }
}

// shipment feedback validation
public class ShipmentFeedbackRequestValidator : AbstractValidator<ShipmentFeedbackRequest>
{
    public ShipmentFeedbackRequestValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPMENT_ID_REQUIRED)
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage(ResponseMessageShipment.SHIPMENT_ID_INVALID);

        RuleFor(x => x.Feedback)
            .MaximumLength(500)
            .WithMessage(ResponseMessageShipment.FEEDBACK_TEXT_TOO_LONG);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage(ResponseMessageShipment.RATING_INVALID);
    }
}

// ChargeableWeightRequest
public class ChargeableWeightRequestValidator : AbstractValidator<ChargeableWeightRequest>
{
    public ChargeableWeightRequestValidator()
    {
        RuleFor(x => x.WidthCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.WIDTH_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.WIDTH_INVALID);

        RuleFor(x => x.HeightCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.HEIGHT_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.HEIGHT_INVALID);

        RuleFor(x => x.LengthCm)
            .NotNull()
            .WithMessage(ResponseMessageParcel.LENGTH_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.LENGTH_INVALID);

        RuleFor(x => x.WeightKg)
            .NotNull()
            .WithMessage(ResponseMessageParcel.WEIGHT_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageParcel.WEIGHT_INVALID);
    }
}