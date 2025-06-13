using MetroShip.Service.ApiModels.MetroLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IMetroLineService
    {
        Task<IEnumerable<MetrolineResponse>> GetAllMetroLine();
    }
}
