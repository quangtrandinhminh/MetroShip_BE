namespace MetroShip.Service.ApiModels.MetroLine;

public record MetroLineItineraryResponse
{
    public string LineId { get; set; }
    public string LineNameVi { get; set; }
    public string LineNameEn { get; set; }
    public string ColorHex { get; set; }
    public decimal BasePriceVndPerKm { get; set; }
}