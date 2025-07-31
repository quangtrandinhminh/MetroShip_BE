using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.Shipment;

public sealed record ItineraryResponse
{
    public string Id { get; set; }
    public int LegOrder { get; set; }
    public RouteResponse Route { get; set; } = null;
    public string? TrainId { get; set; } = null;
    public string? TimeSlotId { get; set; } = null;
    public DateOnly? Date { get; set; } = null;
    public bool IsCompleted { get; set; }
}