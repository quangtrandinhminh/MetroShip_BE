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
        CreateParcelResponse CalculateParcelInfo(ParcelRequest request);
        decimal CalculateShippingCost(ParcelRequest request, double distanceKm, decimal pricePerKm);
        Task<PaginatedListResponse<ParcelResponse>> GetAllParcels(PaginatedListRequest request);
        //Task ConfirmParcelAsync(Guid parcelId);
        //Task RejectParcelAsync(ParcelRejectRequest request);
    }
}
