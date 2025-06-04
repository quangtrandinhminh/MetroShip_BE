namespace MetroShip.Utility.Enums;

public enum ParcelStatusEnum
{
    AwaitingConfirmation,          // Chờ nhân viên duyệt
    AwaitingPayment,               // Chờ thanh toán
    AwaitingDropOff,               // Chờ đem hàng đến ga
    Rejected,                      // Nhân viên từ chối
    Unpaid,                        // Hết hạn thanh toán
    Cancelled,                     // Khách hàng hủy
    AwaitingRefund,                // Chờ hoàn tiền
    Refunded,                      // Đã hoàn tiền
    NoDropOff,                     // Đã thanh toán nhưng không mang hàng đến
    ReceivedAtStationA,            // Nhập kho (ga A)
    InTransitLineXStationXC,       // Trên tuyến X, dừng tại X-C
    TransferringToLineYStationYD,  // Chuyển tuyến Y, dừng tại Y-D
    ReceivedAtStationB,            // Nhập kho (ga B)
    OutForDelivery,                // Xuất kho, chuẩn bị giao
    Overdue,                       // Quá hạn nhận
    LongTermStorage,               // Lưu kho dài hạn (>30d)
    Delivered                      // Đã giao thành công
}

/// <summary>
/// All the triggers (events) that move a parcel between those states.
/// </summary>
public enum ParcelTrigger
{
    // Staff review
    StaffConfirmParcel,          // Staff xác nhận (từ AwaitingConfirmation → AwaitingPayment)
    StaffRejectParcel,           // Staff từ chối (→ Rejected)
    ConfirmationTimeout,         // Hết 24h duyệt (→ Rejected)

    // Payment
    CustomerMakePayment,         // Khách thanh toán (→ AwaitingDropOff)
    PaymentTimeout,              // Hết hạn thanh toán (→ Unpaid)
    CustomerCancel,              // Khách hủy trước drop-off (→ Cancelled)

    // Refund
    RequestRefund,               // Khách yêu cầu hoàn tiền (→ AwaitingRefund)
    ProcessRefund,               // Staff xử lý hoàn tiền (→ Refunded)

    // Drop-off
    CustomerDropOff,             // Khách mang hàng đến ga (→ ReceivedAtStationA)
    DropOffTimeout,              // Hết hạn drop-off (→ NoDropOff)

    // Metro transit
    LoadOntoMetro,               // Staff xếp hàng lên tàu (→ InTransitLineXStationXC)
    ArriveAtTransitStation,      // Tàu đến X-C (→ TransferringToLineYStationYD)
    TransferToNextLine,          // Chuyển tàu tuyến Y (→ ReceivedAtStationB)
    ArriveAtDestinationStation,  // Đến ga B (→ OutForDelivery)

    // Last-mile delivery
    DispatchForDelivery,         // Xuất kho giao (→ OutForDelivery)
    DeliveryTimeout,             // Quá hạn nhận (→ Overdue)
    CustomerReceiveOnTime,       // Khách nhận đúng hạn (→ Delivered)
    CustomerPaySurcharge,        // Khách đóng phụ phí rồi nhận (→ Delivered)
    StorageExpired               // Hết hạn lưu kho (→ LongTermStorage)
}

