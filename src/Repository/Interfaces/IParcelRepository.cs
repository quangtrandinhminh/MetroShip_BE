using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Repository.Base;

namespace MetroShip.Repository.Interfaces
{
    public interface IParcelRepository : IBaseRepository<Parcel>
    {
        Task<List<Parcel>> GetByIdsAsync(IEnumerable<string> parcelIds);
    }
}
