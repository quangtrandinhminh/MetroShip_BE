using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Train
{
    public class TrainPositionResult
    {
        public string TrainId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FromStation { get; set; }
        public string ToStation { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public TimeSpan ETA { get; set; }
        public TimeSpan Elapsed { get; set; }
        public int ProgressPercent { get; set; }
        public string Status { get; set; }
    }
}
