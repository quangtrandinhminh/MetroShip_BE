namespace MetroShip.Service.ApiModels.Region;

public record CreateRegionRequest
{
    public string RegionName { get; set; } 

    public string RegionCode { get; set; } 
}

public record UpdateRegionRequest
{
    public string Id { get; set; }
    public string RegionName { get; set; }
}