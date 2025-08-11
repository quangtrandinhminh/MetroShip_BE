using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Train
{
    public class TrainPositionResult
    {
        public string TrainId { get; set; } = default!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FromStation { get; set; } = default!;
        public string ToStation { get; set; } = default!;
        public DateTimeOffset StartTime { get; set; }
        public TimeSpan ETA { get; set; }
        public TimeSpan Elapsed { get; set; }
        public int ProgressPercent { get; set; }
        public string Status { get; set; } = default!;

        public List<GeoPoint> Path { get; set; } = new();

        public object? AdditionalData { get; set; }
    }
    public class GeoPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
