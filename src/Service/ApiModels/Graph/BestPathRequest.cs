namespace MetroShip.Service.ApiModels.Graph;

public sealed record BestPathRequest
{
    public string DepartureStationId { get; set; }

    public string DestinationStationId { get; set; }
}