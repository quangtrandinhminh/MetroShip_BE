using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.BusinessModels;

namespace MetroShip.Service.ApiModels.Shipment;

public record TotalPriceResponse
{
    public decimal NightDiscount { get; set; }
    public IList<ParcelRequest> ParcelRequests { get; set; } = new List<ParcelRequest>();
    public IList<BestPathGraphResponse> BestPathGraphResponses { get; set; } = new List<BestPathGraphResponse>();
}

