using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Parcel
{
    public class ParcelRejectRequest
    {
        public Guid ParcelId { get; set; }
        public string RejectReason { get; set; } = string.Empty;
    }
}
