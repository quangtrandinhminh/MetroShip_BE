using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Notification
{
    public class SendNotificationToAllRequest
    {
        [Required]
        public string Message { get; set; }

        public string Title { get; set; } = "Thông báo mới";
    }
}
