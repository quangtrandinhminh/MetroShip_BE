﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models;

[Index(nameof(LineId))]
[Index(nameof(FromStationId))]
[Index(nameof(ToStationId))]
[Index(nameof(LineId),nameof(FromStationId),nameof(ToStationId))]
public partial class Route : BaseEntity
{
    public Route()
    {
        RouteCode = this.GetType().Name.ToUpperInvariant();
    }

    [StringLength(20)]
    public string? RouteCode { get; set; }

    [Required]
    [StringLength(50)]
    public string LineId { get; set; }

    [NotMapped]
    public string? LineName { get; set; }

    [Required]
    [StringLength(50)]
    public string FromStationId { get; set; }

    [Required]
    [StringLength(50)]
    public string ToStationId { get; set; }

    [Required]
    [StringLength(100)]
    public string RouteNameVi { get; set; }

    [Required]
    [StringLength(100)]
    public string RouteNameEn { get; set; }

    public int SeqOrder { get; set; }

    public int? TravelTimeMin { get; set; }

    public DirectionEnum Direction { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal LengthKm { get; set; }

    [ForeignKey(nameof(FromStationId))]
    [InverseProperty(nameof(Station.RoutesFrom))]
    public virtual Station FromStation { get; set; }

    [ForeignKey(nameof(ToStationId))]
    [InverseProperty(nameof(Station.RoutesTo))]
    public virtual Station ToStation { get; set; }

    [ForeignKey(nameof(LineId))]
    [InverseProperty(nameof(MetroLine.Routes))]
    public virtual MetroLine MetroLine { get; set; }

    [InverseProperty(nameof(ShipmentItinerary.Route))]
    public virtual ICollection<ShipmentItinerary> ShipmentItineraries { get; set; } = new List<ShipmentItinerary>();
}