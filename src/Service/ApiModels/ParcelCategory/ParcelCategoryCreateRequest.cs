using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.ParcelCategory
{
    public class ParcelCategoryCreateRequest
    {
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsBulk { get; set; }
        public decimal? WeightLimitKg { get; set; }
        public decimal? VolumeLimitCm3 { get; set; }
        public decimal? LengthLimitCm { get; set; }
        public decimal? WidthLimitCm { get; set; }
        public decimal? HeightLimitCm { get; set; }
        public bool IsActive { get; set; }
    }

    public class ParcelCategoryUpdateRequest : ParcelCategoryCreateRequest
    {
        public Guid Id { get; set; }
    }

}
