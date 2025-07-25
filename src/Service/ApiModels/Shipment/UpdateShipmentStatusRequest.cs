using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class UpdateShipmentStatusRequest
    {
        public string TrackingCode { get; set; }
        public string CurrentStationId { get; set; }
    }
}
