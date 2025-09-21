using MetroShip.Service.ApiModels.Region;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.ApiModels.Train;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.MetroLine
{
    public record MetroRouteResponse
    {
        public string Id { get; set; }
        public string LineNameVi { get; set; }
        public string LineNameEn { get; set; }
        public string RegionId { get; set; }
        public string LineCode { get; set; }
        public string ColorHex { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalKm { get; set; }
        public RegionResponse Region { get; set; }
    }

    public record MetroRouteResponseDetails : MetroRouteResponse
    {
        public int? RouteTimeMin { get; set; }
        public int? DwellTimeMin { get; set; }
        public int TotalStations { get; set; }
        public int? LineNumber { get; set; }
        public string LineType { get; set; }
        public string LineOwner { get; set; }
        public List<StationDetailResponse> Stations { get; set; } = new();
        public List<TrainListResponse> Trains { get; set; } = new();
    }
}
