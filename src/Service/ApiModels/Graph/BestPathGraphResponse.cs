using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;
using System.Numerics;

namespace MetroShip.Service.ApiModels.Graph;

public sealed record BestPathGraphResponse
{
    public decimal TotalKm => Routes.Sum(x => x.LengthKm);
    public decimal TotalTime => Routes.Sum(x => x.TravelTimeMin);
    public int TotalStations  => Stations.Count;
    public int TotalRoutes => Routes.Count;
    public decimal ShippingFeeByItinerary => Routes.GroupBy(x => x.LineId)
        .Sum(g => g.Sum(x => x.LengthKm * x.BasePriceVndPerKm));
    public List<RouteResponse> Routes { get; set; } = new List<RouteResponse>();
    public List<StationResponse> Stations { get; set; } = new List<StationResponse>();
    public List<MetroLineItineraryResponse> MetroLines { get; set; } = new List<MetroLineItineraryResponse>();
}