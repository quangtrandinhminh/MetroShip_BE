using MetroShip.Service.ApiModels.MetroTimeSlot;
using MetroShip.Service.ApiModels.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Interfaces
{
    public interface IMetroTimeSlotService
    {
        Task<IList<MetroTimeSlotResponse>> GetAllForMetroTimeSlot();
        Task<PaginatedListResponse<MetroTimeSlotResponse>> GetAllMetroTimeSlot(PaginatedListRequest request);
        Task<string> UpdateMetroTimeSlot(MetroTimeSlotUpdateRequest request);
    }
}
