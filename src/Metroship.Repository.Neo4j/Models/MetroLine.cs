namespace MetroShip.Repository.Neo4j.Models;

public class MetroLine
{
    public int LineId { get; set; }
    public string LineName { get; set; }
    public double TotalKm { get; set; } // in kilometers
    public int TotalStations { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal BasePriceVndPerKm { get; set; }

    // Navigation properties
    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();
}