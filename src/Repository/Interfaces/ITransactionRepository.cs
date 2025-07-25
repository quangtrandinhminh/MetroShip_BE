using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroShip.Repository.Base;
using MetroShip.Repository.Models;

namespace MetroShip.Repository.Interfaces
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
    }
}
