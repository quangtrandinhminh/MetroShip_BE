﻿using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentListResponse
{
    public ShipmentListResponse()
    {
        CurrentStationName = this.DepartureStationName;
    }

    public string Id { get; set; }
    public string TrackingCode { get; set; }
    public string DepartureStationName { get; set; }
    public string DestinationStationName { get; set; }
    public string? CurrentStationName { get; set; }
    public string SenderName { get; set; }
    public string SenderPhone { get; set; }
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public decimal TotalCostVnd { get; set; }
    public DateTimeOffset ScheduledDateTime { get; set; }
    public DateTimeOffset? BookedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? PickupAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public ShipmentStatusEnum ShipmentStatus { get; set; }
}