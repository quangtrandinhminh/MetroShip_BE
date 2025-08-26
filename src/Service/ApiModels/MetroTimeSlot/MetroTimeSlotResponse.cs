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
        public int ScheduleBeforeShiftMinutes { get; set; }= 30;
        public int MaxScheduleBeforeShiftMinutes { get; set; } = 210;
        public TimeOnly? StartReceivingTime { get; set; }
        public TimeOnly? CutOffTime { get; set; }
    }
}
