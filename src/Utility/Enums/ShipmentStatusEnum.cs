using System.ComponentModel;

namespace MetroShip.Utility.Enums;

public enum ShipmentStatusEnum
{
    [Description("Init -> WaitForApproval - System")]
    WaitForApproval = 0,

    [Description("WaitForApproval -> WaitForPayment - Customer")]
    WaitForPayment,

    [Description("WaitForPayment -> Pending - Staff")]
    Pending,

    [Description("Pending -> OnDelivery - Staff/System")]
    OnDelivery,

    [Description("OnDelivery -> OutOfDelivery - Staff")]
    OutOfDelivery,

    [Description("OutOfDelivery -> Received - Staff")]
    Received,

    [Description("Received -> Completed - Customer")]
    Completed,

    [Description("Enum lt 2 -> Rejected - Staff/System")]
    Rejected,

    [Description("Enum lt 2 -> Cancelled - Customer")]
    Cancelled
}