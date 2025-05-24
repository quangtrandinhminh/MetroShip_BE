using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Parcel
{
    public class CreateParcelResponse
    {
        public Guid Id { get; set; }
        public decimal VolumeCm3 { get; set; }
        public decimal ChargeableWeightKg { get; set; }
    }
}
