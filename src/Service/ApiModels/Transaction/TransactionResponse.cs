using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Transaction
{
    public class TransactionResponse
    {
        public string ShipmentId { get; set; }
        public string? PaidById { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public string PaymentTrackingId { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentCurrency { get; set; }
        public DateTimeOffset PaymentTime { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
    }
}
