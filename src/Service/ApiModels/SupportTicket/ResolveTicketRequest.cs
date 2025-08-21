namespace MetroShip.Service.ApiModels.SupportTicket;

public record ResolveTicketRequest
{
    public string TicketId { get; set; }
    public string ResolvedContent { get; set; }
}