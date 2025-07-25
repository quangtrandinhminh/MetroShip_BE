namespace MetroShip.Service.ApiModels.Region;

public record RegionResponse
{
    public string Id { get; set; }
    public string RegionName { get; set; }
    public string RegionCode { get; set; }
}