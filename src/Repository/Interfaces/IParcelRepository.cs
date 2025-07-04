using MetroShip.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Repository.Interfaces
{
    public interface IParcelRepository
    {
        Task<List<Parcel>> GetByIdsAsync(IEnumerable<string> parcelIds);
    }
}
