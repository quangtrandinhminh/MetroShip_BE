namespace MetroShip.Service.ApiModels.Train;

public record TrainListResponse
{
    public string Id { get; set; } = string.Empty;
    public string TrainCode { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed record TrainResponse : TrainListResponse
{
    public int NumberOfCarriages { get; set; }
    public decimal? MaxWeightPerCarriageKg { get; set; }
    public decimal? MaxVolumePerCarriageM3 { get; set; }
    public decimal? CarriageLengthMeter { get; set; }
    public decimal? CarriageWidthMeter { get; set; }
    public decimal? CarriageHeightMeter { get; set; }
    public int? TopSpeedKmH { get; set; }
    public int? TopSpeedUdgKmH { get; set; }
}

public sealed record TrainCurrentCapacityResponse : TrainListResponse
{
    public decimal CurrentWeightKg { get; set; } = 0;
    public decimal CurrentVolumeM3 { get; set; } = 0;
}

