using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentListResponse
{
    public string TrackingCode { get; set; }

    public string DepartureStationId { get; set; }

    public string DestinationStationId { get; set; }

    public int ShipmentStatus { get; set; }

    public decimal TotalCostVnd { get; set; }

    public decimal ShippingFeeVnd { get; set; }
}