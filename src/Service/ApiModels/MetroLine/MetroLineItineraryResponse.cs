namespace MetroShip.Service.ApiModels.MetroLine;

// response cho api thuật toán tìm đường tối ưu
public record MetroLineItineraryResponse
{
    public string Id { get; set; }
    public string LineNameVi { get; set; }
    public string LineNameEn { get; set; }
    public string ColorHex { get; set; }
}