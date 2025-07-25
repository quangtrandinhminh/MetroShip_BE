﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models;

public partial class SystemConfig : BaseEntity
{
    public SystemConfig()
    {
        ConfigKey = this.GetType().Name.ToUpperInvariant();
    }

    [Required]
    [StringLength(100)]
    public string ConfigKey { get; set; }

    public string? ConfigValue { get; set; }

    [StringLength(255)]
    public string Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ConfigTypeEnum ConfigType { get; set; }
}