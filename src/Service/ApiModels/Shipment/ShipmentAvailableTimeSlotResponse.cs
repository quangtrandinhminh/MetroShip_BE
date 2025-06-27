using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentAvailableTimeSlotResponse
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset Date { get; set; }
        public string TimeSlotId { get; set; }
        public string TimeSlotName { get; set; } // ví dụ: "Morning", "Afternoon", "Evening"
        public decimal RemainingWeightKg { get; set; }
        public decimal RemainingVolumeM3 { get; set; }
        public List<string>? ParcelIds { get; set; }
    }
}
