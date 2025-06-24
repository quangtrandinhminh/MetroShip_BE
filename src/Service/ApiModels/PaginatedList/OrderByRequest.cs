namespace MetroShip.Service.ApiModels.PaginatedList;

public record OrderByRequest
{
    public string? OrderBy { get; init; }
    public bool? IsDesc { get; init; }
}