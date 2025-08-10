using MetroShip.Service.ApiModels.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.User
{
    public class UserListWithStatsResponse
    {
        public PaginatedListResponse<UserResponse> Users { get; set; }
        public double PercentageNewUsers { get; set; }
        public int TotalUsersWithRoleUser { get; set; }
    }
}
