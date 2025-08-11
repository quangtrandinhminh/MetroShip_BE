using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Utility.Helpers
{
    public static class TimeSlotHelper
    {
        public static Guid GetCurrentTimeSlotId()
        {
            var now = DateTime.UtcNow.AddHours(7).TimeOfDay;

            if (now >= TimeSpan.Parse("08:00:00") && now < TimeSpan.Parse("11:00:00"))
                return Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6");

            if (now >= TimeSpan.Parse("13:00:00") && now < TimeSpan.Parse("16:00:00"))
                return Guid.Parse("b2c3d4e5-f6a7-8b9c-0d1e-f2a3b4c5d6e7");

            if (now >= TimeSpan.Parse("18:00:00") && now < TimeSpan.Parse("21:00:00"))
                return Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-a3b4c5d6e7f8");

            if (now >= TimeSpan.Parse("23:00:00") || now < TimeSpan.Parse("02:00:00"))
                return Guid.Parse("d4e5f6a7-b8c9-0d1e-2f3a-b4c5d6e7f8a9");

            throw new AppException(ErrorCode.BadRequest, "Không tìm thấy ca phù hợp.");
        }
    }

}