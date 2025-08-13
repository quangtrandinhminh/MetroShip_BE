using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Service.ApiModels.Parcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Utility.Enums;

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
        Task<string> LoadParcelOnTrainAsync(string parcelCode, string trainCode, bool isLost = false);
        Task<string> UnloadParcelFromTrain(string parcelCode, string trainCode, bool isLost = false);
        Task<string> UpdateParcelForAwaitingDeliveryAsync(string parcelCode, bool isLost = false);
        //Task ReportLostParcelAsync(string parcelCode, ShipmentStatusEnum trackingForShipmentStatus);
    }
}
