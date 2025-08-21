using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Notification
{
    public class NotificationUpdateRequest
    {
        [Required]
        public int NotificationId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
