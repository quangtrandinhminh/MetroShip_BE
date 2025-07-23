#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroShip.Repository.Models.Base;
using MetroShip.Repository.Models.Identity;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models;

public partial class ShipmentMedia : BaseEntity
{
    [StringLength(50)]
    public string ShipmentId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public string? Description { get; set; }
    public BusinessMediaTypeEnum BusinessMediaType { get; set; }
    public MediaTypeEnum MediaType { get; set; }

    [ForeignKey(nameof(ShipmentId))]
    [InverseProperty(nameof(Shipment.ShipmentMedias))]
    public virtual Shipment Shipment { get; set; }
}