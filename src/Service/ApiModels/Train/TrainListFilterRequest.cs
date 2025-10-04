using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Train;

public sealed record TrainListFilterRequest : PaginatedListRequest
{
    public string? Id { get; set; }
    public string? TrainCode { get; set; }
    public string? StationId { get; set; }

    public string? LineId { get; set; }

    public string? TimeSlotId { get; set; }

    public DateOnly? Date { get; set; }

    public string? ModelName { get; set; }

    public bool? IsAvailable { get; set; }

    public DirectionEnum? Direction { get; set; }

    public TrainStatusEnum? Status { get; set; }
}