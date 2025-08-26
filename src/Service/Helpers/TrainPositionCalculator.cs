using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Train;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Enums;
using MetroShip.Utility.Exceptions;
using MetroShip.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroShip.Service.Helpers
{
    public static class TrainPositionCalculator
    {
        public static TrainPositionResult CalculatePosition(
            MetroTrain train,
            List<RoutePolylineDto> routePolylines,
            DateTimeOffset startTime,
            int segmentIndex,
            DirectionEnum direction,
            double avgSpeedKmH)
        {
            if (train.Line == null || train.Line.Routes == null || !train.Line.Routes.Any())
                throw new ArgumentException("Train has no line or routes.");

            var elapsed = DateTimeOffset.UtcNow - startTime;
            var totalKmTravelled = (elapsed.TotalHours * avgSpeedKmH);

            var activeRoutes = train.Line.Routes
                .Where(r => r.Direction == direction)
                .OrderBy(r => r.SeqOrder)
                .ToList();

            if (segmentIndex >= activeRoutes.Count)
                segmentIndex = activeRoutes.Count - 1;

            var remainingRoutes = activeRoutes.Skip(segmentIndex).ToList();
            var flatPath = BuildGeoPoints(remainingRoutes, routePolylines);

            double travelled = 0;
            GeoPoint currentPos = flatPath.First();

            for (int i = 1; i < flatPath.Count; i++)
            {
                var prev = flatPath[i - 1];
                var curr = flatPath[i];

                var segDist = GeoUtils.Haversine(prev.Latitude, prev.Longitude, curr.Latitude, curr.Longitude);

                if (travelled + segDist >= totalKmTravelled)
                {
                    var remain = totalKmTravelled - travelled;
                    var fraction = segDist == 0 ? 0 : remain / segDist;

                    var (lat, lon) = GeoUtils.Interpolate(prev.Latitude, prev.Longitude, curr.Latitude, curr.Longitude, fraction);
                    currentPos = new GeoPoint { Latitude = lat, Longitude = lon };
                    break;
                }

                travelled += segDist;
                currentPos = curr;
            }

            var totalLineKm = activeRoutes.Sum(r => r.LengthKm);
            var eta = (double)totalLineKm / avgSpeedKmH;

            var currentRoute = activeRoutes[segmentIndex];

            return new TrainPositionResult
            {
                TrainId = train.Id,
                Latitude = currentPos.Latitude,
                Longitude = currentPos.Longitude,
                FromStation = currentRoute.FromStation?.StationNameVi ?? "Unknown",
                ToStation = currentRoute.ToStation?.StationNameVi ?? "Unknown",
                StartTime = startTime,
                ETA = TimeSpan.FromHours(eta),
                Elapsed = elapsed,
                ProgressPercent = totalLineKm == 0 ? 0 : (int)((travelled / (double)totalLineKm) * 100),
                Status = train.Status.ToString(),
                Path = flatPath
            };
        }

        /// <summary>
        /// Dùng cho AdditionalData.FullPath → JSON đầy đủ
        /// </summary>
        public static List<object> BuildFullPath(List<Route> routes, List<RoutePolylineDto> polylines)
        {
            return routes.Select(r =>
            {
                var polyline = polylines.FirstOrDefault(p =>
                    p.FromStation == r.FromStation.StationNameVi &&
                    p.ToStation == r.ToStation.StationNameVi &&
                    p.Direction == r.Direction);

                return new
                {
                    FromStation = r.FromStation.StationNameVi,
                    ToStation = r.ToStation.StationNameVi,
                    SeqOrder = r.SeqOrder,
                    Direction = r.Direction,
                    Status = false,
                    Polyline = polyline != null && polyline.Polyline.Any()
                    ? polyline.Polyline.Select(p => new
                    {
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    }).ToList<object>()
                    : new List<object>()
                };
            }).ToList<object>();
        }


        /// <summary>
        /// Dùng cho tính toán (flatten polyline thành list lat/lon)
        /// </summary>
        private static List<GeoPoint> BuildGeoPoints(List<Route> routes, List<RoutePolylineDto> polylines)
        {
            var result = new List<GeoPoint>();

            foreach (var route in routes)
            {
                var polyline = polylines.FirstOrDefault(p =>
                    p.FromStation == route.FromStation.Id &&
                    p.ToStation == route.ToStation.Id &&
                    p.Direction == route.Direction);

                if (polyline != null && polyline.Polyline.Any())
                {
                    if (result.Count > 0 &&
                        result.Last().Latitude == polyline.Polyline.First().Latitude &&
                        result.Last().Longitude == polyline.Polyline.First().Longitude)
                    {
                        result.AddRange(polyline.Polyline.Skip(1));
                    }
                    else
                    {
                        result.AddRange(polyline.Polyline);
                    }
                }
                else
                {
                    if (route.FromStation?.Latitude != null && route.FromStation?.Longitude != null)
                        result.Add(new GeoPoint { Latitude = route.FromStation.Latitude.Value, Longitude = route.FromStation.Longitude.Value });

                    if (route.ToStation?.Latitude != null && route.ToStation?.Longitude != null)
                        result.Add(new GeoPoint { Latitude = route.ToStation.Latitude.Value, Longitude = route.ToStation.Longitude.Value });
                }
            }

            return result;
        }
    }
}