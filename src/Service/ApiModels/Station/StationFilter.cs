namespace MetroShip.Service.ApiModels.Station;

public record StationFilter
{
    public string? StationName { get; set; }
    public string? RegionId { get; set; }
    public bool? IsActive { get; set; }
    public string? MetroRouteId { get; set; }
}