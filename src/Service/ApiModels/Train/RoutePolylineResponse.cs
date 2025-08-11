using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Train
{
    public class RoutePolylineDto
    {
        public string FromStation { get; set; } = null!;
        public string ToStation { get; set; } = null!;
        public int SeqOrder { get; set; }
        public DirectionEnum Direction { get; set; }
        public List<GeoPoint> Polyline { get; set; } = new();
    }
}
