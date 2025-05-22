using FluentValidation;
using FluentValidation.Results;
using MetroShip.Repository.Interfaces;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.Helpers;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Validations;

public class ShipmentValidator
{
    private readonly IValidator<ShipmentRequest> _shipmentRequestValidator;
    private readonly IValidator<ShipmentItineraryRequest> _itineraryRequestValidator;
    private readonly IValidator<ParcelRequest> _parcelRequestValidator;

    public ShipmentValidator()
    {
        _shipmentRequestValidator = new ShipmentRequestValidator();
        _itineraryRequestValidator = new ShipmentItineraryRequestValidator();
        _parcelRequestValidator = new ParcelRequestValidator();
    }

    public void ValidateShipmentRequest(ShipmentRequest request, DateTimeOffset minBookDate, DateTimeOffset maxBookDate)
    {
        _shipmentRequestValidator.ValidateApiModel(request);
        if (request.ScheduledDateTime < minBookDate || request.ScheduledDateTime > maxBookDate)
        {
            throw new AppException(
               ErrorCode.BadRequest,
               $"The ScheduledDateTime must be between {minBookDate} and {maxBookDate}.",
               StatusCodes.Status400BadRequest);
        }

        foreach (var parcel in request.Parcels)
        {
            _parcelRequestValidator.ValidateApiModel(parcel);
        }

        foreach (var itinerary in request.ShipmentItineraries)
        {
            _itineraryRequestValidator.ValidateApiModel(itinerary);
        }
    }
}

public class ShipmentRequestValidator : AbstractValidator<ShipmentRequest>
{
    public ShipmentRequestValidator()
    {
        RuleFor(x => x.ScheduledDateTime)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPMENT_DATE_REQUIRED);

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

        RuleFor(x => x.RecipientNationalId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.RECIPIENT_NATIONAL_ID_REQUIRED)
            .Matches(@"^\d{9,12}$")
            .WithMessage(ResponseMessageIdentity.PHONENUMBER_LENGTH_INVALID);

        RuleFor(x => x.TotalCostVnd)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_INVALID);

        RuleFor(x => x.InsuranceFeeVnd)
            .GreaterThan(0).WithMessage(ResponseMessageShipment.TOTAL_COST_VND_INVALID)
            .When(x => x.InsuranceFeeVnd.HasValue);

        RuleFor(x => x.ShippingFeeVnd)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPPING_FEE_VND_REQUIRED)
            .GreaterThan(0)
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

    }
}

public class ShipmentItineraryRequestValidator : AbstractValidator<ShipmentItineraryRequest>
{
    public ShipmentItineraryRequestValidator()
    {
        RuleFor(x => x.RouteId)
            .NotNull()
            .WithMessage(ResponseMessageItinerary.ROUTE_ID_REQUIRED);

        RuleFor(x => x.LegOrder)
            .NotNull()
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_REQUIRED)

            .WithMessage(ResponseMessageItinerary.LEG_ORDER_INVALID);

        /*RuleFor(x => x.EstMinutes)
            .GreaterThan(0)
            .WithMessage(ResponseMessageItinerary.EST_MINUTES_INVALID);*/
    }
}

public class ParcelRequestValidator : AbstractValidator<ParcelRequest>
{
    public ParcelRequestValidator()
    {
        RuleFor(x => x.ParcelCategoryId)
            .NotNull()
            .WithMessage(ResponseMessageParcel.PARCEL_CATEGORY_ID_REQUIRED);

        RuleFor(x => x.WidthCm)
            .NotNull()
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_REQUIRED)
            .GreaterThan(0)
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_INVALID);

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

        RuleFor(x => x.IsBulk)
            .NotNull()
            .WithMessage(ResponseMessageParcel.IS_BULK_REQUIRED)
            .Must(x => x == true || x == false)
            .WithMessage(ResponseMessageParcel.IS_BULK_INVALID);
    }
}

