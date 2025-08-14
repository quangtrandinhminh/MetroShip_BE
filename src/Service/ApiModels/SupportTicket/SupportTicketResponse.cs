using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.SupportTicket;

public record SupportTicketResponse
{
    public string Id { get; set; }
    public string OpenById { get; set; }

    public string? ResolvedById { get; set; }

    public string ShipmentId { get; set; }

    public string Subject { get; set; }

    public string Description { get; set; }

    public string? ResolvedContent { get; set; }

    public SupportTypeEnum SupportType { get; set; }

    public SupportTicketStatusEnum Status { get; set; }

    public DateTimeOffset OpenedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public string? ClosedBy { get; set; }
}