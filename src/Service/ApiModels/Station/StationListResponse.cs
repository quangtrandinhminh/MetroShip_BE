using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Station
{
    public class StationListResponse
    {
        public string StationId { get; set; }
        public string StationCode { get; set; }
        public string StationNameVi { get; set; }
        public string StationNameEn { get; set; }
        public bool IsActive { get; set; }
        public string RegionId { get; set; }
    }
}
