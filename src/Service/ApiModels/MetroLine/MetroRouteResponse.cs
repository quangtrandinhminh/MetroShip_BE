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
    }

    public record MetroRouteResponseDetails : MetroRouteResponse
    {

    }
}
