using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.MetroLine
{
    public class MetroLineDto
    {
        public string Id { get; set; }
        public string RegionId { get; set; }
        public string LineNameVi { get; set; }
        public string LineNameEn { get; set; }
        public string LineCode { get; set; }
        public int? LineNumber { get; set; }
        public string LineType { get; set; }
        public string LineOwner { get; set; }
        public decimal TotalKm { get; set; }
        public int TotalStations { get; set; }
        public int? RouteTimeMin { get; set; }
        public int? DwellTimeMin { get; set; }
        public string ColorHex { get; set; }
        public bool IsActive { get; set; }
        public List<StationDto> Stations { get; set; } = new();
        public List<RoutePathDto> RoutePaths { get; set; } = new();
    }

    public class StationDto
    {
        public string Id { get; set; }
        public string StationCode { get; set; }
        public string StationNameVi { get; set; }
        public string StationNameEn { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SeqOrder { get; set; }
    }

    public class RoutePathDto
    {
        public string FromStationId { get; set; }
        public string ToStationId { get; set; }
        public List<GeoPoint> InterpolatedPoints { get; set; } = new();
    }

    public class GeoPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
