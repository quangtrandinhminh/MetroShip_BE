using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentLocationResponse
    {
        public string TrackingCode { get; set; }

        public string TrainId { get; set; }

        public string TrainCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string CurrentStationId { get; set; }

        public string CurrentStationName { get; set; }

        public string ShipmentStatus { get; set; }

        public DateTimeOffset? EstimatedArrivalTime { get; set; }

        public List<ParcelTrackingDto> ParcelTrackingHistory { get; set; } = new();
    }

    public class ParcelTrackingDto
    {
        public string ParcelCode { get; set; }

        public string Status { get; set; }

        public string StationId { get; set; }

        public string StationName { get; set; }

        public DateTimeOffset EventTime { get; set; }

        public string Note { get; set; }
    }
}
