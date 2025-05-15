using System.ComponentModel;

namespace MetroShip.Utility.Enums;

public enum ShiftEnum
{
    [Description("Morning")]
    Morning = 1,

    [Description("Afternoon")]
    Afternoon,

    [Description("Evening")]
    Evening,

    [Description("Night")]
    Night
}