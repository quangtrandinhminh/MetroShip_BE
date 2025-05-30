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
    private readonly IValidator<TotalPriceCalcRequest> _totalPriceCalcRequestValidator;

    public ShipmentValidator()
    {
        _shipmentRequestValidator = new ShipmentRequestValidator();
        _itineraryRequestValidator = new ShipmentItineraryRequestValidator();
        _parcelRequestValidator = new ParcelRequestValidator();
        _totalPriceCalcRequestValidator = new TotalPriceCalcRequestValidator();
    }

    public void ValidateShipmentRequest(ShipmentRequest request)
    {
        _shipmentRequestValidator.ValidateApiModel(request);

        foreach (var parcel in request.Parcels)
        {
            _parcelRequestValidator.ValidateApiModel(parcel);
        }

        foreach (var itinerary in request.ShipmentItineraries)
        {
            _itineraryRequestValidator.ValidateApiModel(itinerary);
        }
    }

    public void ValidateTotalPriceCalcRequest(TotalPriceCalcRequest request)
    {
        _totalPriceCalcRequestValidator.ValidateApiModel(request);

        foreach (var parcel in request.Parcels)
        {
            _parcelRequestValidator.ValidateApiModel(parcel);
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
            // .NotEmpty()
            // .WithMessage(ResponseMessageShipment.RECIPIENT_NATIONAL_ID_REQUIRED)
            .Matches(@"^\d{9,12}$")
            .WithMessage(ResponseMessageShipment.RECIPIENT_NATIONAL_ID_INAVLID);

        RuleFor(x => x.TotalCostVnd)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_REQUIRED)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageShipment.TOTAL_COST_VND_INVALID);

        RuleFor(x => x.InsuranceFeeVnd)
            .GreaterThanOrEqualTo(0).WithMessage(ResponseMessageShipment.INSURANCE_FEE_VND_INVALID)
            .When(x => x.InsuranceFeeVnd.HasValue);

        RuleFor(x => x.ShippingFeeVnd)
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

    }
}

public class ShipmentItineraryRequestValidator : AbstractValidator<ShipmentItineraryRequest>
{
    public ShipmentItineraryRequestValidator()
    {
        RuleFor(x => x.RouteId)
            .NotNull()
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage(ResponseMessageItinerary.ROUTE_ID_REQUIRED);

        RuleFor(x => x.LegOrder)
            .NotNull()
            .WithMessage(ResponseMessageItinerary.LEG_ORDER_REQUIRED)

            .WithMessage(ResponseMessageItinerary.LEG_ORDER_INVALID);

        RuleFor(x => x.BasePriceVndPerKm)
            .NotNull()
            .WithMessage(ResponseMessageItinerary.BASE_PRICE_VND_PER_KM_REQUIRED)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ResponseMessageItinerary.BASE_PRICE_VND_PER_KM_INVALID);

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
    }
}

public class TotalPriceCalcRequestValidator : AbstractValidator<TotalPriceCalcRequest>
{
    public TotalPriceCalcRequestValidator()
    {
        RuleFor(x => x.DepartureStationId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.DEPARTURE_STATION_ID_REQUIRED);

        RuleFor(x => x.DestinationStationId)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.DESTINATION_STATION_ID_REQUIRED);

        RuleFor(x => x.ScheduleShipmentDate)
            .NotEmpty()
            .WithMessage(ResponseMessageShipment.SHIPMENT_DATE_REQUIRED);

        RuleFor(x => x.Parcels)
            .NotEmpty()
            .WithMessage(ResponseMessageParcel.PARCEL_REQUIRED);
    }
}