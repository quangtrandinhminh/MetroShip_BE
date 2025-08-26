namespace MetroShip.Repository.Extensions;

public record NearbyStationIds
{
    public string StationId { get; init; } = string.Empty;
    public double DistanceMeters { get; init; } = 0;
}