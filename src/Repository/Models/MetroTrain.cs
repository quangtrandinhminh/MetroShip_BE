#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models;
public partial class MetroTrain : BaseEntity
{
    public MetroTrain()
    {
        TrainCode = this.GetType().Name.ToUpperInvariant();
    }

    [Required]
    [StringLength(20)]
    public string TrainCode { get; set; }

    [StringLength(50)]
    public string? ModelName { get; set; }

    [Required]
    [StringLength(50)]
    public string LineId { get; set; }

    public bool IsActive { get; set; }

    [StringLength(50)]
    public string? CurrentStationId { get; set; }

    [NotMapped]
    public string? CurrentStationName { get; set; }

    public TrainStatusEnum Status { get; set; }

    public int NumberOfCarriages { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? MaxWeightPerCarriageKg { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? MaxVolumePerCarriageM3 { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? CarriageLengthMeter { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? CarriageWidthMeter { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? CarriageHeightMeter { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public int? TopSpeedKmH { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public int? TopSpeedUdgKmH { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [ForeignKey(nameof(LineId))]
    [InverseProperty(nameof(MetroLine.Trains))]
    public virtual MetroLine Line { get; set; }

    [InverseProperty(nameof(ShipmentItinerary.Train))]
    public virtual ICollection<ShipmentItinerary> ShipmentItineraries { get; set; } = new HashSet<ShipmentItinerary>();

    [InverseProperty(nameof(TrainSchedule.Train))]
    public virtual ICollection<TrainSchedule> TrainSchedules { get; set; } = new List<TrainSchedule>();

    public bool IsTrainCodeEven()
    {
        // format: regionCode-linecode-Tnumber, e.g: HCMC-L01-T01
        var parts = TrainCode.Split('-');
        if (parts.Length < 3)
        {
            Console.WriteLine("Invalid TrainCode format. Expected format: regionCode-linecode-Tnumber");
            return false;
        }

        // Get "T01"
        var numberPart = parts[2];

        // Delete "T" (không phân biệt hoa thường)
        if (numberPart.StartsWith("T", StringComparison.OrdinalIgnoreCase))
        {
            numberPart = numberPart.Substring(1);
        }

        // Bây giờ mới chuyển đổi chuỗi số thành số nguyên
        if (int.TryParse(numberPart, out int trainNumber))
        {
            return trainNumber % 2 == 0;
        }

        Console.WriteLine($"Invalid number format in TrainCode part: {parts[2]}");
        return false;
    }
}