using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Shipment;

namespace MetroShip.Service.Interfaces;

public interface IItineraryService
{
    Task<DateTimeOffset> CheckEstArrivalTime(BestPathGraphResponse pathResponse, string currentSlotId, DateOnly date);
    Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(
        string shipmentId, int maxAttempt);

   Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request);
}