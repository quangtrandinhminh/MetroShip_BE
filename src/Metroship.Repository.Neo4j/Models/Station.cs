namespace MetroShip.Repository.Neo4j.Models;

public class Station
{
    public int StationId { get; set; }
    public string StationNameVi { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string RegionId { get; set; }
    public bool IsActive { get; set; }
}