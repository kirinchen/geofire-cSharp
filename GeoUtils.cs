using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire.util {
    public class GeoUtils {

        private GeoUtils() { }

        public static double distance(GeoLocation location1, GeoLocation location2) {
            return distance(location1.latitude, location1.longitude, location2.latitude, location2.longitude);
        }

        public static double distance(double lat1, double long1, double lat2, double long2) {
            // Earth's mean radius in meters
             double radius = (Constants.EARTH_EQ_RADIUS + Constants.EARTH_POLAR_RADIUS) / 2;
            double latDelta = DegreeToRadian(lat1 - lat2);
            double lonDelta = DegreeToRadian(long1 - long2);

            double a = (Math.Sin(latDelta / 2) * Math.Sin(latDelta / 2)) +
                       (Math.Cos(DegreeToRadian(lat1)) * Math.Cos(DegreeToRadian(lat2)) *
                               Math.Sin(lonDelta / 2) * Math.Sin(lonDelta / 2));
            return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegreeToRadian(double angle) {
            return Math.PI * angle / 180.0;
        }

        public static double distanceToLatitudeDegrees(double distance) {
            return distance / Constants.METERS_PER_DEGREE_LATITUDE;
        }

        public static double distanceToLongitudeDegrees(double distance, double latitude) {
            double radians = DegreeToRadian(latitude);
            double numerator = Math.Cos(radians) * Constants.EARTH_EQ_RADIUS * Math.PI / 180;
            double denominator = 1 / Math.Sqrt(1 - Constants.EARTH_E2 * Math.Sin(radians) * Math.sin(radians));
            double deltaDegrees = numerator * denominator;
            if (deltaDegrees < Constants.EPSILON) {
                return distance > 0 ? 360 : distance;
            } else {
                return Math.Min(360, distance / deltaDegrees);
            }
        }

        public static double wrapLongitude(double longitude) {
            if (longitude >= -180 && longitude <= 180) {
                return longitude;
            }
            double adjusted = longitude + 180;
            if (adjusted > 0) {
                return (adjusted % 360.0) - 180;
            } else {
                return 180 - (-adjusted % 360);
            }
        }

    }
}
