namespace MetroShip.Service.ApiModels.SystemConfig;

public record ChangeConfigValueRequest
{
    public string ConfigKey { get; set; }

    public string ConfigValue { get; set; }
}