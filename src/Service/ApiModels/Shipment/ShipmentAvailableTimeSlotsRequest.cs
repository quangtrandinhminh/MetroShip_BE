using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentAvailableTimeSlotsRequest
    {
        public string TrackingCode { get; set; }
        public int? MaxAttempts { get; set; } = 3; // số ca thử dời tối đa
    }
}
