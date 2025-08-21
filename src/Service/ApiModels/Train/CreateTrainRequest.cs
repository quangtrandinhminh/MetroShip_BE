using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Train;

public record CreateTrainRequest
{
    public string? TrainCode { get; set; } = null;
    public string? ModelName { get; set; }
    public bool IsActive { get; set; }
    public string LineId { get; set; }
    public int TrainNumber { get; set; }
    public string? CurrentStationId { get; set; }
    public int NumberOfCarriages { get; set; }
    public decimal? MaxWeightPerCarriageKg { get; set; }
    public decimal? MaxVolumePerCarriageM3 { get; set; }
    public decimal? CarriageLengthMeter { get; set; }
    public decimal? CarriageWidthMeter { get; set; }
    public decimal? CarriageHeightMeter { get; set; }
    public int? TopSpeedKmH { get; set; }
    public int? TopSpeedUdgKmH { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}