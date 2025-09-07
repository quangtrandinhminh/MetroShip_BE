using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Station
{
    public class CreateStationRequest
    {
        public string? Id { get; set; }
        public string? StationCode { get; set; }
        public string? StationNameVi { get; set; }
        public string? StationNameEn { get; set; }
        public string? Address { get; set; }
        public bool? IsUnderground { get; set; }
        public bool? IsActive { get; set; }
        public string? RegionId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal ToNextStationKm { get; set; } = 0;
    }
}
