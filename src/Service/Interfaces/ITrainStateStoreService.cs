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
        Task SetDirectionAsync(string trainId, DirectionEnum direction);
        Task<DirectionEnum?> GetDirectionAsync(string trainId);

        Task SetSegmentIndexAsync(string trainId, int index);
        Task<int?> GetSegmentIndexAsync(string trainId);

        Task SetStartTimeAsync(string trainId, DateTimeOffset startTime);
        Task<DateTimeOffset?> GetStartTimeAsync(string trainId);

        Task SetPositionResultAsync(string trainId, TrainPositionResult result);
        Task<TrainPositionResult?> GetPositionResultAsync(string trainId);
    }
}
