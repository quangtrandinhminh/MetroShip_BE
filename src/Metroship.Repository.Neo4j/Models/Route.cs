using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Neo4j.Models;

public class Route
{
    public int RouteId { get; set; }
    public int FromStationId { get; set; }
    public int ToStationId { get; set; }
    public double LengthKm { get; set; }
    public DirectionEnum Direction { get; set; }
    public bool IsActive { get; set; } = true;
}