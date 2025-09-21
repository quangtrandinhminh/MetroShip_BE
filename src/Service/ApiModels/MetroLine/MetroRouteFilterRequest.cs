namespace MetroShip.Service.ApiModels.MetroLine;

public record MetroRouteFilterRequest
{
    public string? RegionId { get; set; }
    public bool? IsActive { get; set; }
    public string? LineNameVi { get; set; }
    public string? LineNameEn { get; set; }
    public string? LineCode { get; set; }
}