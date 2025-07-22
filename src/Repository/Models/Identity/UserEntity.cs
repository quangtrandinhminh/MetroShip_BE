using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text.Json.Serialization;
using MetroShip.Repository.Models.Base;
using MetroShip.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Serilog.Parsing;

namespace MetroShip.Repository.Models.Identity;

public class UserEntity : IdentityUser
{
    public UserEntity()
    {
        Id = Guid.NewGuid().ToString();
        CreatedTime = LastUpdatedTime = CoreHelper.SystemTimeNow;
    }

    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public DateOnly? BirthDate { get; set; }

    public virtual ICollection<UserRoleEntity> UserRoles { get; set; }
    //public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

    // Import Account from VietQR
    [MaxLength(6)]
    public int? BankId { get; set; }

    [MaxLength(19)]
    public string? AccountNo { get; set; }

    [MaxLength(255)]
    public string? AccountName { get; set; }

    // Base Property
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    // Identity Property
    public DateTimeOffset? Verified { get; set; }
    public string? OTP { get; set; }
    public bool IsActive => PhoneConfirmed;
    public bool PhoneConfirmed => Verified.HasValue;
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiredTime { get; set; }
    public bool IsRefreshTokenExpired => CoreHelper.SystemTimeNow >= RefreshTokenExpiredTime;


    [InverseProperty(nameof(Report.User))]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty(nameof(Shipment.Sender))]
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    [InverseProperty(nameof(SupportingTicket.User))]
    public virtual ICollection<SupportingTicket> SupportingTickets { get; set; } = new List<SupportingTicket>();

    [InverseProperty(nameof(Notification.User))]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty(nameof(Transaction.PaidBy))]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [InverseProperty(nameof(StaffAssignment.Staff))]
    public virtual ICollection<StaffAssignment?> StaffAssignments { get; set; }

    [NotMapped]
    public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
    [NotMapped]
    public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }
}

public class RoleEntity : IdentityRole
{

    public RoleEntity()
    {
        Id = Guid.NewGuid().ToString();
    }
    public virtual ICollection<UserRoleEntity> UserRoles { get; set; }

    [NotMapped]
    public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

    [NotMapped]
    public override string NormalizedName { get => base.NormalizedName; set => base.NormalizedName = value; }
}

public class UserRoleEntity : IdentityUserRole<string>
{
    public virtual UserEntity User { get; set; }
    public virtual RoleEntity Role { get; set; }
}

/*
public class RefreshToken : BaseEntity
{
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTimeOffset Expires { get; set; }
    public bool IsExpired => CoreHelper.SystemTimeNow >= Expires;
    public bool IsActive => !IsExpired;
}
*/

