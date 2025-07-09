using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentItineraryResponseDto
    {
        public int LegOrder { get; set; }
        public string RouteId { get; set; }
        public string? TrainId { get; set; }
        public string? TrainCode { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string? TimeSlotId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
