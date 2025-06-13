using MetroShip.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.MetroTimeSlot
{
    public class MetroTimeSlotResponse
    {
        public string Id { get; set; }
        public DayOfWeekEnum? DayOfWeek { get; set; }
        public DateOnly? SpecialDate { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public ShiftEnum Shift { get; set; }
        public bool IsAbnormal { get; set; }
    }
}
