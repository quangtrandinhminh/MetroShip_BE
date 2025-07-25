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

public partial class MetroTimeSlot : BaseEntity
{
    public DayOfWeekEnum? DayOfWeek { get; set; }

    public DateOnly? SpecialDate { get; set; }

    public bool IsAbnormal { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    public ShiftEnum Shift { get; set; }

    [InverseProperty(nameof(ShipmentItinerary.TimeSlot))]
    public virtual ICollection<ShipmentItinerary> ShipmentItineraries { get; set; } = new List<ShipmentItinerary>();
}