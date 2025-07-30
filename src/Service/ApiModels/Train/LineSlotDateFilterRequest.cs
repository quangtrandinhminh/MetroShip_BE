using MetroShip.Service.ApiModels.PaginatedList;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MetroShip.Service.ApiModels.Train;

public sealed record LineSlotDateFilterRequest : PaginatedListRequest
{
    [FromRoute]
    public string LineId { get; set; }

    [FromRoute]
    public string TimeSlotId { get; set; }

    [FromRoute]
    public DateOnly Date { get; set; }
}