using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {

    public class GeoLocationUtils {

        public interface Handler {
            GeoLocation parse(object o);
        }
        private static GeoLocationUtils instance;

        private Handler handler;

        private GeoLocationUtils(Handler h) {
            handler = h;
        }

        public GeoLocation parse(object o) {
            return handler.parse(o);
        }

        public static void initInstance(Handler h) {
            instance = new GeoLocationUtils(h);
        }

        public static GeoLocationUtils getInstance() {
            return instance;
        }


    }
}
