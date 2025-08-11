using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset SenddAt { get; set; }
    }
}
