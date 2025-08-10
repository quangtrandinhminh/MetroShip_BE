using MetroShip.Service.ApiModels.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentListWithStatsResponse
    {
        public PaginatedListResponse<ShipmentListResponse> Shipments { get; set; }
        public int TotalShipments { get; set; }
        public double PercentageNewShipments { get; set; }
        public int TotalCompleteShipments { get; set; }
        public double PercentageNewCompleteShipments { get; set; }
        public List<ParcelCategoryDto> ParcelCategoryStats { get; set; }
    }
    public class ParcelCategoryDto
    {
        public Guid ParcelCategoryId { get; set; }
        public string ParcelCategoryName { get; set; }
        public int Total { get; set; }
        public int NewToday { get; set; }
        public double PercentageNew { get; set; }
    }
}
