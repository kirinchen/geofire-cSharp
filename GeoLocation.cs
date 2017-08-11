using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.surfm.firebase.geofire {
    public class GeoLocation {
        /** The latitude of this location in the range of [-90, 90] */
        public readonly double latitude;

        /** The longitude of this location in the range of [-180, 180] */
        public readonly double longitude;

        /**
         * Creates a new GeoLocation with the given latitude and longitude.
         *
         * @throws java.lang.IllegalArgumentException If the coordinates are not valid geo coordinates
         * @param latitude The latitude in the range of [-90, 90]
         * @param longitude The longitude in the range of [-180, 180]
         */
        public GeoLocation(double latitude, double longitude) {
            if (!GeoLocation.coordinatesValid(latitude, longitude)) {
                throw new System.Exception("Not a valid geo location: " + latitude + ", " + longitude);
            }
            this.latitude = latitude;
            this.longitude = longitude;
        }

        /**
         * Checks if these coordinates are valid geo coordinates.
         * @param latitude The latitude must be in the range [-90, 90]
         * @param longitude The longitude must be in the range [-180, 180]
         * @return True if these are valid geo coordinates
         */
        public static bool coordinatesValid(double latitude, double longitude) {
            return (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180);
        }

        public override bool Equals(object o) {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            GeoLocation that = (GeoLocation)o;

            if (!double.Equals(that.latitude, latitude) ) return false;
            if (!double.Equals(that.longitude, longitude) ) return false;

            return true;
        }

        public override int GetHashCode() {
            int result;
            long temp;
            temp = BitConverter.DoubleToInt64Bits(latitude);
            result = (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(longitude);
            result = 31 * result + (int)(temp ^ (temp >> 32));
            return result;
        }


        public override string ToString() {
            return "GeoLocation(" + latitude + ", " + longitude + ")";
        }


    }
}


