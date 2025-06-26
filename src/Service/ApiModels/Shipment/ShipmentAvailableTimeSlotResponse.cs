using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentAvailableTimeSlotResponse
    {
        public DateTimeOffset Date { get; set; }
        public string TimeSlotId { get; set; } = default!;
        public string TimeSlotName { get; set; } = default!;
        public decimal RemainingWeightKg { get; set; }
        public decimal RemainingVolumeM3 { get; set; }
    }
}
