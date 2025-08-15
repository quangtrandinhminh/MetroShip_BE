namespace MetroShip.Service.ApiModels.Region;

public record CreateRegionRequest
{
    public string RegionName { get; set; } 

    public string RegionCode { get; set; } 
}