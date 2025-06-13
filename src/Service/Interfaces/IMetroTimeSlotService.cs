using MetroShip.Service.ApiModels.MetroTimeSlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IMetroTimeSlotService
    {
        Task<IEnumerable<MetroTimeSlotResponse>> GetAllForMetroTimeSlot();

    }
}
