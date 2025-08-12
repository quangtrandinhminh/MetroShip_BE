using MetroShip.Utility.Enums;
using System.ComponentModel.DataAnnotations;

namespace MetroShip.Service.ApiModels.SupportTicket;

public sealed record SupportTicketRequest
{
    public string ShipmentId { get; set; }

    public string Subject { get; set; }

    public string Description { get; set; }

    public SupportTypeEnum SupportType { get; set; }
}