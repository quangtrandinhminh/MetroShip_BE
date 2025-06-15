using MetroShip.Service.Helpers;
using MetroShip.Utility.Enums;

namespace MetroShip.Service.ApiModels.Transaction;

public sealed record TransactionEnumResponse
{
    public IList<EnumResponse> EnumPaymentStatusResponses => EnumHelper.GetEnumList<PaymentStatusEnum>();
    public IList<EnumResponse> EnumTransactionTypeResponses => EnumHelper.GetEnumList<TransactionTypeEnum>();
}