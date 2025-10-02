using JNJServices.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNJServices.Utility.Helper
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusMiles = 3958.8;

        // Helper method to convert degrees to radians
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Haversine formula to calculate the distance between two points
        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            double dlat = lat2Rad - lat1Rad;
            double dlon = lon2Rad - lon1Rad;

            double a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dlon / 2) * Math.Sin(dlon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMiles * c;
        }

        // Method to calculate the total distance traveled along a path
        public static double CalculateTotalDistance(List<Coordinate> path)
        {
            double totalDistance = 0.0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                double lat1 = path[i].Latitude;
                double lon1 = path[i].Longitude;
                double lat2 = path[i + 1].Latitude;
                double lon2 = path[i + 1].Longitude;

                totalDistance += Haversine(lat1, lon1, lat2, lon2);
            }

            return totalDistance;
        }
    }
}
