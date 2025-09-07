using MetroShip.Repository.Base;
using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface ITrainScheduleRepository : IBaseRepository<TrainSchedule>
{
    Task<IList<TrainSchedule>> GetTrainSchedulesByTrainListAsync(IList<string> trainIds);
    Task<DirectionEnum?> GetTrainDirectionByTrainAndSegmentAsync(string trainId, int segmentIndex, DirectionEnum direction);
}