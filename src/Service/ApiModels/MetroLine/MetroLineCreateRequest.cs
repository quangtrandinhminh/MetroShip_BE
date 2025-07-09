using MetroShip.Service.ApiModels.Station;

namespace MetroShip.Service.ApiModels.MetroLine;

public record MetroLineCreateRequest
{
    public string LineNameVi { get; set; } = string.Empty;
    public string LineNameEn { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string? LineCode { get; set; } = string.Empty;

    // if the station is existed just add Id, else Id is null and provide all fields to create a new station
    public IList<CreateStationRequest> Stations { get; set; } = new List<CreateStationRequest>();
}
