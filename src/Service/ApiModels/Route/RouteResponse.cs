using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.Route;

public sealed record RouteResponse
{
    public string RouteId { get; set; }
    public string RouteName { get; set; }
    public int SeqOrder { get; set; }
    public decimal LengthKm { get; set; }
    public int TravelTimeMin { get; set; }
    public string FromStationId { get; set; }
    public string ToStationId { get; set; }
    public string LineId { get; set; }
}