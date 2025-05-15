namespace MetroShip.Service.ApiModels.PaginatedList;

public class PaginatedListResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}