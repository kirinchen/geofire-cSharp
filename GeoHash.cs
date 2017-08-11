using com.surfm.firebase.geofire.util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.surfm.firebase.geofire.core {

    public class GeoHash {

        private readonly string geoHash;

    // The default precision of a geohash
    private static readonly int DEFAULT_PRECISION = 10;

        // The maximal precision of a geohash
        public static readonly int MAX_PRECISION = 22;

        // The maximal number of bits precision for a geohash
        public static readonly int MAX_PRECISION_BITS = MAX_PRECISION * Base32Utils.BITS_PER_BASE32_CHAR;

        public GeoHash(double latitude, double longitude) : this(latitude, longitude, DEFAULT_PRECISION) {
        }

        public GeoHash(GeoLocation location): this(location.latitude, location.longitude, DEFAULT_PRECISION) {
        }

        public GeoHash(double latitude, double longitude, int precision) {
            if (precision < 1) {
                throw new System.Exception("Precision of GeoHash must be larger than zero!");
            }
            if (precision > MAX_PRECISION) {
                throw new System.Exception("Precision of a GeoHash must be less than " + (MAX_PRECISION + 1) + "!");
            }
            if (!GeoLocation.coordinatesValid(latitude, longitude)) {
                throw new System.Exception(string.Format("Not valid location coordinates: [{0}, {1}]", latitude, longitude));
            }
            double[] longitudeRange = { -180, 180 };
            double[] latitudeRange = { -90, 90 };

            char[] buffer = new char[precision];

            for (int i = 0; i < precision; i++) {
                int hashValue = 0;
                for (int j = 0; j < Base32Utils.BITS_PER_BASE32_CHAR; j++) {
                    bool even = (((i * Base32Utils.BITS_PER_BASE32_CHAR) + j) % 2) == 0;
                    double val = even ? longitude : latitude;
                    double[] range = even ? longitudeRange : latitudeRange;
                    double mid = (range[0] + range[1]) / 2;
                    if (val > mid) {
                        hashValue = (hashValue << 1) + 1;
                        range[0] = mid;
                    } else {
                        hashValue = (hashValue << 1);
                        range[1] = mid;
                    }
                }
                buffer[i] = Base32Utils.valueToBase32Char(hashValue);
            }
            this.geoHash = new string(buffer);
        }

        public GeoHash(string hash) {
            if (hash.Length == 0 || !Base32Utils.isValidBase32string(hash)) {
                throw new System.Exception("Not a valid geoHash: " + hash);
            }
            this.geoHash = hash;
        }

        public string getGeoHashstring() {
            return this.geoHash;
        }


        public override bool Equals(object o) {
            if (this == o) return true;
            if (o == null ||GetType() != o.GetType()) return false;

            GeoHash other = (GeoHash)o;

            return this.geoHash.Equals(other.geoHash);
        }


        public override string ToString() {
            return "GeoHash{" +
                    "geoHash='" + geoHash + '\'' +
                    '}';
        }

        public override int GetHashCode() {
            return this.geoHash.GetHashCode();
        }

    }

}


