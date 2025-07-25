﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models;

[Index(nameof(LineCode), nameof(RegionId), IsUnique = true)]
public partial class MetroLine : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RegionId { get; set; }

    [Required]
    [StringLength(255)]
    public string LineNameVi { get; set; }

    [Required]
    [StringLength(255)]
    public string LineNameEn { get; set; }

    [StringLength(20)]
    public string LineCode { get; set; }

    public int? LineNumber { get; set; }

    [StringLength(255)]
    public string? LineType { get; set; }

    [StringLength(255)]
    public string? LineOwner { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal TotalKm { get; set; }

    public int TotalStations { get; set; }

    // This property is used to calculate est time
    //public int? MinHeadwayMin { get; set; }

    // This property is used to calculate timeline
    //public int? MaxHeadwayMin { get; set; }

    public int? RouteTimeMin { get; set; }

    public int? DwellTimeMin { get; set; }

    public string? StationListJSON { get; set; } // JSON string of coordinates

    public List<StationListItem> StationList =>
        string.IsNullOrEmpty(StationListJSON) ? new List<StationListItem>() :
        System.Text.Json.JsonSerializer.Deserialize<List<StationListItem>>(StationListJSON);

    [StringLength(20)]
    public string ColorHex { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(RegionId))]
    [InverseProperty(nameof(Region.MetroLines))]
    public virtual Region Region { get; set; }

    [InverseProperty(nameof(Route.MetroLine))]
    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();

    [InverseProperty(nameof(MetroTrain.Line))]
    public virtual ICollection<MetroTrain> Trains { get; set; } = new List<MetroTrain>();
}

[NotMapped]
public class StationListItem
{
    public string StationId { get; set; }
    public string StationCode { get; set; }
}