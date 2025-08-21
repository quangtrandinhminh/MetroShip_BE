using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Train
{
    public class TrainDto
    {
        public string Id { get; set; }
        public string TrainCode { get; set; }
        public string? CurrentStationId { get; set; }
        public TrainStatusEnum Status { get; set; }
        public object CurrentStationLat { get; internal set; }
        public object CurrentStationLng { get; internal set; }
    }
}
