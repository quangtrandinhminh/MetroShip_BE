using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Station;
using System.Numerics;

namespace MetroShip.Service.ApiModels.Graph;

public sealed record BestPathGraphResponse
{
    public decimal TotalShippingFeeVnd => Parcels.Sum(x => x.ShippingFeeVnd ?? 0);
    public decimal TotalInsuranceFeeVnd => Parcels.Sum(x => x.InsuranceFeeVnd ?? 0);
    public decimal TotalCostVnd => Parcels.Sum(x => x.PriceVnd ?? 0);
    public decimal TotalKm => Routes.Sum(x => x.LengthKm);
    //public decimal TotalTime => Routes.Sum(x => x.TravelTimeMin);
    public DateTimeOffset? EstArrivalTime { get; set; }
    public int TotalStations  => Stations.Count;
    public int TotalRoutes => Routes.Count;
    /*public decimal ShippingFeeByItinerary => Routes.GroupBy(x => x.LineId)
        .Sum(g => g.Sum(x => x.LengthKm * x.BasePriceVndPerKg));*/
    public int TotalParcels => Parcels.Count;
    public int TotalMetroLines => MetroLines.Count;
    public List<RouteStationResponse> Routes { get; set; } = new List<RouteStationResponse>();
    public List<StationResponse> Stations { get; set; } = new List<StationResponse>();
    public List<MetroLineItineraryResponse> MetroLines { get; set; } = new List<MetroLineItineraryResponse>();
    public List<ParcelRequest> Parcels { get; set; } = new List<ParcelRequest>();
}