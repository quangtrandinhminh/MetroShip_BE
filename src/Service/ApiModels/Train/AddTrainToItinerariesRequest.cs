using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.Service.ApiModels.Train;

public sealed record AddTrainToItinerariesRequest
{
    public string TrainId { get; set; }
    //public string LineId { get; set; }
    public string TimeSlotId { get; set; }
    public DateOnly Date { get; set; }
    public DirectionEnum Direction { get; set; }
}