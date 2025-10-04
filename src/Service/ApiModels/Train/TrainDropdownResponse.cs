namespace MetroShip.Service.ApiModels.Train;

public record TrainDropdownResponse
{
    public string Id { get; set; } = string.Empty;
    public string TrainCode { get; set; } = string.Empty;
}