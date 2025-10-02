using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.MetroTimeSlot;

public record MetroTimeSlotUpdateRequest
{
    public string Id { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public TimeOnly StartReceivingTime { get; set; }
    public TimeOnly CutOffTime { get; set; }
}