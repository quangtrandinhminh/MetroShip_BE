namespace MetroShip.Service.ApiModels.PaginatedList;

public record PaginatedListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}