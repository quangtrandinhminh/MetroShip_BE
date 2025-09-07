using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.SystemConfig;

public record SystemConfigResponse
{
    public string Id { get; set; }

    public string ConfigKey { get; set; }

    public string? ConfigValue { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ConfigTypeEnum ConfigType { get; set; }

    public string ConfigTypeName => ConfigType.ToString();

    public string? CreatedBy { get; set; }

    public string? LastUpdatedBy { get; set; }

    public string? DeletedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastUpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}