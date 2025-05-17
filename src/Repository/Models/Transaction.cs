#nullable disable
using MetroShip.Repository.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MetroShip.Repository.Models.Identity;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Models;

public partial class Transaction : BaseEntity
{
    [StringLength(50)]
    public string? PaidById { get; set; }

    [StringLength(50)]
    public PaymentMethodEnum PaymentMethod { get; set; }

    public PaymentStatusEnum PaymentStatus { get; set; }

    [StringLength(50)]
    public string PaymentTrackingId { get; set; }

    public DateTimeOffset PaymentDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaymentAmount { get; set; }

    [StringLength(20)]
    public string PaymentCurrency { get; set; }

    public DateTimeOffset PaymentTime { get; set; }

    public TransactionTypeEnum TransactionType { get; set; }

    [ForeignKey(nameof(PaidById))]
    public virtual UserEntity? PaidBy { get; set; }
}