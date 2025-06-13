using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.BusinessModels;

namespace MetroShip.Service.ApiModels.Shipment;

public record TotalPriceResponse
{
    public decimal NightDiscount { get; set; }
    public IList<ParcelRequest> ParcelRequests { get; set; } = new List<ParcelRequest>();
    public BestPathGraphResponse Standard { get; set; }
    public BestPathGraphResponse? Nearest { get; set; }
    public BestPathGraphResponse? Cheapest { get; set; }
}

