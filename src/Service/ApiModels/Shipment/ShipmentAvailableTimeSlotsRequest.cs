using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentAvailableTimeSlotsRequest
    {
        public string RouteId { get; set; } = default!;
        public DateTimeOffset StartDate { get; set; }
        public int MaxAttempts { get; set; } = 3;
        public List<string> ParcelIds { get; set; } = new();
    }
}
