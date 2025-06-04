using System.ComponentModel.DataAnnotations;
using MetroShip.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Models.Base
{
    [Index(nameof(Id), IsUnique = true)]
    [Index(nameof(CreatedAt))]
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = IdGenerator();
            CreatedAt = LastUpdatedAt = CoreHelper.SystemTimeNow;
        }

        [Key]
        public string Id { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }

        [StringLength(50)]
        public string? DeletedBy { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        string IdGenerator()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
