using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.MetroLine
{
    public class MetrolineGetByRegionResponse
    {
        public string Id { get; set; }
        public string LineNameVi { get; set; }
        public string LineNameEn { get; set; }
        public decimal BasePriceVndPerKm { get; set; }
        public string regionCode { get; set; }
    }
}
