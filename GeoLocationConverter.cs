using com.surfm.firebase.geofire.core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {

    public class GeoLocationConverter {

        public interface Convert {
            GeoLocation parse(object o);

            object toFireObj(string hash, GeoLocation gl);
        }
        private static GeoLocationConverter instance;

        private Convert handler;

        private GeoLocationConverter(Convert h) {
            handler = h;
        }

        public GeoLocation parse(object o) {
            return handler.parse(o);
        }

        public object toFireObj(string v, GeoLocation location) {
            return handler.toFireObj(v, location);
        }

        public static void initInstance(Convert h) {
            instance = new GeoLocationConverter(h);
        }

        public static GeoLocationConverter getInstance() {
            if (instance == null) {
                initInstance(new SampleHandler());
            }
            return instance;
        }


        internal class SampleHandler : Convert {
            public static GeoLocation _parse(object o) {
                try {
                    Dictionary<string, object> os = (Dictionary<string, object>)o;
                    List<object> ls = (List<object>)os["l"];
                    if (ls.Count != 2) return null;
                    double latitude = (double)ls[0];
                    double longitude = (double)ls[1];
                    if (GeoLocation.coordinatesValid(latitude, longitude)) {
                        return new GeoLocation(latitude, longitude);
                    } else {
                        return null;
                    }
                } catch (Exception e) {
                    return null;
                }

            }

            public GeoLocation parse(object o) {
                return _parse(o);
            }

            public object toFireObj(string hash, GeoLocation location) {
                Dictionary<string, object> updates = new Dictionary<string, object>();
                updates.Add("g", hash);
                updates.Add("l", new List<object>(new object[] { location.latitude, location.longitude }));
                return updates;
            }
        }


    }



}
