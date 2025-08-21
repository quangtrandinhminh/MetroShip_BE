namespace MetroShip.Service.ApiModels.Station;

public record NearbyStationsRequest
{
    public double UserLatitude { get; init; }
    public double UserLongitude { get; init; }
    /*public int MaxDistanceInMeters { get; init; } = 1000;
    public int MaxCount { get; init; } = 10;*/
}