using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.BusinessModels;

namespace MetroShip.Service.ApiModels.Shipment;

public record TotalPriceResponse
{
    //public IList<ParcelRequest> ParcelRequests { get; set; } = new List<ParcelRequest>();
    public BestPathGraphResponse Standard { get; set; }
    public BestPathGraphResponse? Nearest { get; set; }
    public BestPathGraphResponse? Shortest { get; set; }
    public int StationsInDistanceMeter { get; set; }
}

