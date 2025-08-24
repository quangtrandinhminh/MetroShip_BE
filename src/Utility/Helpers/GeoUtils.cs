using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Utility.Helpers
{
    public static class GeoUtils
    {
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0; // Earth radius in km
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(DegreesToRadians(lat1)) *
                    Math.Cos(DegreesToRadians(lat2)) *
                    Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public static (double lat, double lon) Interpolate(double lat1, double lon1, double lat2, double lon2, double fraction)
        {
            return (
                lat1 + (lat2 - lat1) * fraction,
                lon1 + (lon2 - lon1) * fraction
            );
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static bool IsNear(double lat1, double lon1, double lat2, double lon2, double thresholdMeters = 50)
        {
            // Calculate distance using Haversine formula
            double distance = Haversine(lat1, lon1, lat2, lon2) * 1000; // Convert km to meters
            return distance <= thresholdMeters;
        }
    }
}
