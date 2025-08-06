using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IParcelService
    {
        //CreateParcelResponse CalculateParcelInfo(ParcelRequest request);
        //decimal CalculateShippingCost(ParcelRequest request, double distanceKm, decimal pricePerKm);
        Task<PaginatedListResponse<ParcelResponse>> GetAllParcels(PaginatedListRequest request);
        Task<ParcelResponse?> GetParcelByParcelCodeAsync(string parcelCode);
        Task ConfirmParcelAsync(ParcelConfirmRequest request);
        //Task RejectParcelAsync(ParcelRejectRequest request);
        Task LoadParcelOnTrainAsync(string parcelCode, string trainCode);
        Task UnloadParcelFromTrain(string parcelCode, string trainCode);
        Task UpdateParcelForAwaitingDeliveryAsync(string parcelCode);
    }
}
