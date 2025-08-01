using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.Shipment;

public record ItineraryGraphResponse
{
    public decimal TotalKm => Routes.Sum(x => x.LengthKm);
    public int TotalStations => Stations.Count;
    public int TotalRoutes => Routes.Count;
    public int TotalMetroLines => MetroLines.Count;
    public List<RouteStationResponse> Routes { get; set; } = new List<RouteStationResponse>();
    public List<StationResponse> Stations { get; set; } = new List<StationResponse>();
    public List<MetroLineItineraryResponse> MetroLines { get; set; } = new List<MetroLineItineraryResponse>();
}