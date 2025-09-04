using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.SystemConfig;

public class SystemConfigRequest
{
    public string Id { get; set; }

    public string Description { get; set; }

    public ConfigTypeEnum ConfigType { get; set; } = ConfigTypeEnum.System;
}