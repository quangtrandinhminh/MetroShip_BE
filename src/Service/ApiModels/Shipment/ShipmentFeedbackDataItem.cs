using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.ApiModels.Shipment
{
    public class ShipmentFeedbackDataItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public double PositiveFeedbackRate { get; set; }
        public double ResponseRate { get; set; }
        public double AvgResponseTimeHours { get; set; }
    }
}
