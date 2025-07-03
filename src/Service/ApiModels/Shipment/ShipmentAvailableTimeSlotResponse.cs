using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Service.ApiModels.MetroTimeSlot;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentAvailableTimeSlotResponse
    {
        public string Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string TimeSlotId { get; set; }
        public string TimeSlotName { get; set; } // ví dụ: "Morning", "Afternoon", "Evening"
        public MetroTimeSlotResponse? SlotDetail { get; set; } // Ensure MetroTimeSlot is a valid type
    }
}
