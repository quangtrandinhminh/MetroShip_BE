namespace MetroShip.Service.ApiModels.Station;

public sealed record StationResponse
{
    public string StationId { get; set; }
    public string StationNameVi { get; set; }
    public string StationNameEn { get; set; }
    public bool IsUnderground { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}