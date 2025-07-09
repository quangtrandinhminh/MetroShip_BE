using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class UpdateShipmentStatusResponse : ShipmentLocationResponse
    {
        public string Message { get; set; }
    }
}
