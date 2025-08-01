using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Shipment;

public record ShipmentMediaResponse
{
    public string Id { get; set; } = null!;
    public string ShipmentId { get; set; } = null!;
    public string MediaUrl { get; set; } = null!;
    public string? Description { get; set; }
    public BusinessMediaTypeEnum BusinessMediaType { get; set; }
    public MediaTypeEnum MediaType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string LastUpdatedBy { get; set; } = null!;
}