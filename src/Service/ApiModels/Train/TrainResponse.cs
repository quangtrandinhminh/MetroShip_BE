using MetroShip.Repository.Models;
using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroShip.Service.ApiModels.Train;

public record TrainListResponse
{
    public string Id { get; set; } = string.Empty;
    public string TrainCode { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CurrentTimeSlotId { get; set; }
    public string? CurrentStationId { get; set; }
    public string? CurrentStationName { get; set; }
    public string? CurrentRouteStationId { get; set; }
    public string? NextStationId { get; set; }
    public string? NextStationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public TrainStatusEnum Status { get; set; }
    public DirectionEnum? Direction { get; set; }
    public string StatusName => Status.ToString();
    public IList<TrainScheduleResponse> TrainSchedules { get; set; } = new List<TrainScheduleResponse>();
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

public sealed record TrainScheduleResponse
{
    public string Id { get; set; } 
    public string TrainId { get; set; }

    public string TimeSlotId { get; set; }

    public ShiftEnum Shift { get; set; }

    public string LineId { get; set; }

    public string? LineName { get; set; }

    public string DepartureStationId { get; set; }

    public string? DepartureStationName { get; set; }

    public string DestinationStationId { get; set; }

    public string? DestinationStationName { get; set; }

    public DirectionEnum Direction { get; set; }
}

