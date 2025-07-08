using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Train
{
    public class TrackingLocationUpdateDto
    {
        public string TrainId { get; set; }
        public string TrackingCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string StationId { get; set; } // optional
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
