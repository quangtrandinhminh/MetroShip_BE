using MetroShip.Service.ApiModels.Train;
using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface ITrainStateStoreService
    {
        #region Set methods
        Task SetDirectionAsync(string trainId, DirectionEnum direction);
        Task SetSegmentIndexAsync(string trainId, int index);
        Task SetStartTimeAsync(string trainId, DateTimeOffset startTime);
        Task SetPositionResultAsync(string trainId, TrainPositionResult result);
        #endregion

        #region Get methods
        Task<DirectionEnum?> GetDirectionAsync(string trainId);
        Task<int?> GetSegmentIndexAsync(string trainId);
        Task<DateTimeOffset?> GetStartTimeAsync(string trainId);
        Task<TrainPositionResult?> GetPositionResultAsync(string trainId);
        #endregion

        #region Remove methods
        Task RemoveDirectionAsync(string trainId);
        Task RemoveSegmentIndexAsync(string trainId);
        Task RemoveStartTimeAsync(string trainId);
        Task RemovePositionResultAsync(string trainId);
        Task RemoveAllTrainStateAsync(string trainId);
        #endregion

        #region has methods
        Task<bool> HasDirectionAsync(string trainId);
        Task<bool> HasSegmentIndexAsync(string trainId);
        Task<bool> HasStartTimeAsync(string trainId);
        Task<bool> HasPositionResultAsync(string trainId);
        #endregion

        #region Shipment Tracking
        Task SetShipmentTrackingAsync(string trackingCode, string trainId, TrainPositionResult position);
        Task<dynamic?> GetShipmentTrackingAsync(string trackingCode);
        Task RemoveShipmentTrackingAsync(string trackingCode);
        Task<bool> HasShipmentTrackingAsync(string trackingCode);
        #endregion

        #region List methods
        Task<List<string>> GetAllActiveTrainIdsAsync();
        #endregion
    }
}
