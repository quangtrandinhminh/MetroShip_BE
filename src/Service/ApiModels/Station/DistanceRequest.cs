namespace MetroShip.Service.ApiModels.Station;

public record DistanceRequest
{
    public double FromLatitude { get; set; }
    public double FromLongitude { get; set; }

    public double ToLatitude { get; set; }
    public double ToLongitude { get; set; }
}