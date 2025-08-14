using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Service.ApiModels.Insurance;

namespace MetroShip.Service.ApiModels.ParcelCategory
{
    public class ParcelCategoryResponse
    {
        public string Id { get; set; }
        public string CategoryName { get; set; }
        public string? CategoryInsuranceId { get; set; }
        public string? Description { get; set; }
        public decimal? WeightLimitKg { get; set; }
        public decimal? VolumeLimitCm3 { get; set; }
        public decimal? LengthLimitCm { get; set; }
        public decimal? WidthLimitCm { get; set; }
        public decimal? HeightLimitCm { get; set; }
        public decimal? TotalSizeLimitCm { get; set; }
        public decimal? InsuranceRate { get; set; }
        public decimal? InsuranceFeeVnd { get; set; }
        public bool IsInsuranceRequired { get; set; }
        public IList<CategoryInsuranceResponse> CategoryInsurances { get; set; } = new List<CategoryInsuranceResponse>();
    }
}
