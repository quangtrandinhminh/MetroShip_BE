using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Notification
{
    public class NotificationCreateRequest
    {
        [Required]
        public string? ToUserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
