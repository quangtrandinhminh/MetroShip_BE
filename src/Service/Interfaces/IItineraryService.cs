using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Shipment;

namespace MetroShip.Service.Interfaces;

public interface IItineraryService
{
    //Task<DateTimeOffset> CheckEstArrivalTime(BestPathGraphResponse pathResponse, string currentSlotId, DateOnly date);
    Task<List<ItineraryResponse>> CheckAvailableTimeSlotsAsync(
        Shipment shipment, int maxAttempt);

   Task<TotalPriceResponse> GetItineraryAndTotalPrice(TotalPriceCalcRequest request);
   Task HandleItineraryForReturnShipment(Shipment primaryShipment, Shipment returnShipment);
   Task<string> ChangeItinerariesToNextSlotAsync(ChangeItinerarySlotRequest request);
   Task<DateTimeOffset> CheckEstArrivalTime(ItineraryResponse itineraryResponse);
}