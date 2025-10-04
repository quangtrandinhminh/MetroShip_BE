using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ItineraryResponse
{
    public string Id { get; set; }
    public int LegOrder { get; set; }
    public string? MetroRouteId { get; set; }
    public string? MetroRouteName { get; set; }
    public DirectionEnum? Direction { get; set; }
    public string? DirectionName => Direction.ToString();
    public string RouteId { get; set; } = null!;
    public string? TrainId { get; set; } = null;
    public string? TrainCode { get; set; } = null;
    public string? TimeSlotId { get; set; } = null;
    public ShiftEnum? Shift { get; set; } = null;
    public string? ShiftName => Shift.ToString();
    public DateOnly? Date { get; set; } = null;
    public bool IsCompleted { get; set; }
    public string? Message { get; set; }
}