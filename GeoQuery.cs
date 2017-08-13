using com.surfm.firebase.geofire;
using com.surfm.firebase.geofire.core;
using com.surfm.firebase.geofire.util;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {
    public class GeoQuery {
        internal object Lock = new object();
        public class LocationInfo {
            public readonly GeoLocation location;
            public readonly bool inGeoQuery;
            public readonly GeoHash geoHash;

            public LocationInfo(GeoLocation location, bool inGeoQuery) {
                this.location = location;
                this.inGeoQuery = inGeoQuery;
                this.geoHash = new GeoHash(location);
            }
        }




        private readonly GeoQueryChildEventListener childEventLister;
        private readonly GeoFire geoFire;
        private readonly HashSet<GeoQueryEventListener> eventListeners = new HashSet<GeoQueryEventListener>();
        private readonly Dictionary<GeoHashQuery, Query> firebaseQueries = new Dictionary<GeoHashQuery, Query>();
        private readonly HashSet<GeoHashQuery> outstandingQueries = new HashSet<GeoHashQuery>();
        private Dictionary<string, LocationInfo> locationInfos = new Dictionary<string, LocationInfo>();
        private GeoLocation center;
        private double radius;
        private HashSet<GeoHashQuery> queries;





        /**
         * Creates a new GeoQuery object centered at the given location and with the given radius.
         * @param geoFire The GeoFire object this GeoQuery uses
         * @param center The center of this query
         * @param radius The radius of this query, in kilometers
         */
        internal GeoQuery(GeoFire geoFire, GeoLocation center, double radius) {
            this.geoFire = geoFire;
            this.center = center;
            // convert from kilometers to meters
            this.radius = radius * 1000;
            childEventLister = new GeoQueryChildEventListener(this);
        }

        private bool locationIsInQuery(GeoLocation location) {
            return GeoUtils.distance(location, center) <= this.radius;
        }

        private void updateLocationInfo(string key, GeoLocation location) {
            LocationInfo oldInfo = GeoUtils.getMapSafe(key, locationInfos);
            bool isNew = (oldInfo == null);
            bool changedLocation = (oldInfo != null && !oldInfo.location.Equals(location));
            bool wasInQuery = (oldInfo != null && oldInfo.inGeoQuery);

            bool isInQuery = this.locationIsInQuery(location);
            if ((isNew || !wasInQuery) && isInQuery) {
                foreach (GeoQueryEventListener listener in this.eventListeners) {
                    this.geoFire.raiseEvent(() => {
                        listener.onKeyEntered(key, location);
                    });
                }
            } else if (!isNew && changedLocation && isInQuery) {
                foreach (GeoQueryEventListener listener in this.eventListeners) {
                    this.geoFire.raiseEvent(() => {
                        listener.onKeyMoved(key, location);
                    });

                }
            } else if (wasInQuery && !isInQuery) {
                foreach (GeoQueryEventListener listener in this.eventListeners) {
                    this.geoFire.raiseEvent(() => {
                        listener.onKeyExited(key);
                    });

                }
            }
            LocationInfo newInfo = new LocationInfo(location, this.locationIsInQuery(location));
            //this.locationInfos.Add(key, newInfo);
            GeoUtils.setMapSafe(key, newInfo, locationInfos);
        }

        private bool geoHashQueriesContainGeoHash(GeoHash geoHash) {
            if (this.queries == null) {
                return false;
            }
            foreach (GeoHashQuery query in this.queries) {
                if (query.containsGeoHash(geoHash)) {
                    return true;
                }
            }
            return false;
        }

        private void reset() {


            foreach (Query q in firebaseQueries.Values) {
                childEventLister.removeEventListener(q);
            }

            this.outstandingQueries.Clear();
            this.firebaseQueries.Clear();
            this.queries = null;
            this.locationInfos.Clear();
        }

        private bool hasListeners() {
            return this.eventListeners.Count > 0;
        }

        private bool canFireReady() {
            return this.outstandingQueries.Count <= 0;
        }

        private void checkAndFireReady() {
            if (canFireReady()) {
                foreach (GeoQueryEventListener l in eventListeners) {
                    geoFire.raiseEvent(() => {
                        l.onGeoQueryReady();
                    });
                }
            }
        }

        private void addValueToReadyListener(Query firebase, GeoHashQuery query) {

            ValueChangedListenerSetup vs = new ValueChangedListenerSetup(firebase, true, (e) => {
                lock (Lock) {
                    if (e.DatabaseError == null) {
                        this.outstandingQueries.Remove(query);
                        this.checkAndFireReady();
                    } else {
                        foreach (GeoQueryEventListener listener in eventListeners) {
                            geoFire.raiseEvent(() => {
                                listener.onGeoQueryError(e.DatabaseError);
                            });
                        }
                    }
                }
            });


        }

        private void setupQueries() {
            HashSet<GeoHashQuery> oldQueries = (this.queries == null) ? new HashSet<GeoHashQuery>() : this.queries;
            HashSet<GeoHashQuery> newQueries = GeoHashQuery.queriesAtLocation(center, radius);
            this.queries = newQueries;
            foreach (GeoHashQuery query in oldQueries) {
                if (!newQueries.Contains(query)) {
                    //firebaseQueries[(query)].removeEventListener(this.childEventLister);
                    childEventLister.removeEventListener(GeoUtils.getMapSafe(query, firebaseQueries));
                    firebaseQueries.Remove(query);
                    outstandingQueries.Remove(query);
                }
            }
            foreach (GeoHashQuery query in newQueries) {
                if (!oldQueries.Contains(query)) {
                    outstandingQueries.Add(query);
                    DatabaseReference databaseReference = this.geoFire.getDatabaseReference();
                    Query firebaseQuery = databaseReference.OrderByChild("g").StartAt(query.getStartValue()).EndAt(query.getEndValue());
                    childEventLister.addChildEventListener(firebaseQuery);
                    addValueToReadyListener(firebaseQuery, query);
                    //firebaseQueries.Add(query, firebaseQuery);
                    GeoUtils.setMapSafe(query, firebaseQuery, firebaseQueries);
                }
            }
            foreach (string key in locationInfos.Keys) {
                updateLocationInfo(key, GeoUtils.getMapSafe(key, locationInfos).location);
            }
            // remove locations that are not part of the geo query anymore
            List<string> keys = new List<string>(locationInfos.Keys);
            keys.ForEach(k => {
                LocationInfo v = GeoUtils.getMapSafe(k, locationInfos);
                if (!geoHashQueriesContainGeoHash(v.geoHash)) {
                    locationInfos.Remove(k);
                }
            });
            checkAndFireReady();
        }

        internal void childAdded(DataSnapshot dataSnapshot) {
            GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
            if (location != null) {
                this.updateLocationInfo(dataSnapshot.Key, location);
            } else {
                // throw an error in future?
            }
        }

        internal void childChanged(DataSnapshot dataSnapshot) {
            GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
            if (location != null) {
                this.updateLocationInfo(dataSnapshot.Key, location);
            } else {
                // throw an error in future?
            }
        }

        internal void childRemoved(DataSnapshot dataSnapshot) {
            string key = dataSnapshot.Key;
            LocationInfo info = GeoUtils.getMapSafe(key, locationInfos);
            if (info != null) {
                ValueChangedListenerSetup vs = new ValueChangedListenerSetup(geoFire.getDatabaseRefForKey(key), true, (s) => {
                    if (s.DatabaseError == null) {
                        lock (Lock) {
                            GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
                            GeoHash hash = (location != null) ? new GeoHash(location) : null;
                            if (hash == null || !geoHashQueriesContainGeoHash(hash)) {
                                LocationInfo _info = GeoUtils.getMapSafe(key, locationInfos);
                                locationInfos.Remove(key);
                                if (_info != null && _info.inGeoQuery) {
                                    foreach (GeoQueryEventListener listener in eventListeners) {
                                        geoFire.raiseEvent(() => {
                                            listener.onKeyExited(key);
                                        });
                                    }
                                }
                            }
                        }
                    }
                });

            }
        }

        /**
         * Adds a new GeoQueryEventListener to this GeoQuery.
         *
         * @throws java.lang.IllegalArgumentException If this listener was already added
         *
         * @param listener The listener to add
         */
        public void addGeoQueryEventListener(GeoQueryEventListener listener) {
            lock (Lock) {

                if (eventListeners.Contains(listener)) {
                    throw new Exception("Added the same listener twice to a GeoQuery!");
                }
                eventListeners.Add(listener);
                if (this.queries == null) {
                    this.setupQueries();
                } else {
                    foreach (string key in locationInfos.Keys) {
                        LocationInfo info = GeoUtils.getMapSafe(key, locationInfos);
                        if (info.inGeoQuery) {
                            geoFire.raiseEvent(() => {
                                listener.onKeyEntered(key, info.location);
                            });
                        }
                    }


                    if (canFireReady()) {
                        geoFire.raiseEvent(() => {
                            listener.onGeoQueryReady();
                        });
                    }
                }
            }
        }

        /**
         * Removes an event listener.
         *
         * @throws java.lang.IllegalArgumentException If the listener was removed already or never added
         *
         * @param listener The listener to remove
         */
        public void removeGeoQueryEventListener(GeoQueryEventListener listener) {
            lock (Lock) {
                if (!eventListeners.Contains(listener)) {
                    throw new Exception("Trying to remove listener that was removed or not added!");
                }
                eventListeners.Remove(listener);
                if (!this.hasListeners()) {
                    reset();
                }
            }
        }

        /**
         * Removes all event listeners from this GeoQuery.
         */
        public void removeAllListeners() {
            lock (Lock) {
                eventListeners.Clear();
                reset();
            }
        }

        /**
         * Returns the current center of this query.
         * @return The current center
         */
        public GeoLocation getCenter() {
            lock (Lock) {
                return center;
            }
        }

        /**
         * Sets the new center of this query and triggers new events if necessary.
         * @param center The new center
         */
        public void setCenter(GeoLocation center) {
            lock (Lock) {
                this.center = center;
                if (this.hasListeners()) {
                    this.setupQueries();
                }
            }
        }

        /**
         * Returns the radius of the query, in kilometers.
         * @return The radius of this query, in kilometers
         */
        public double getRadius() {
            // convert from meters
            lock (Lock) {
                return radius / 1000;
            }
        }

        /**
         * Sets the radius of this query, in kilometers, and triggers new events if necessary.
         * @param radius The new radius value of this query in kilometers
         */
        public void setRadius(double radius) {
            // convert to meters
            lock (Lock) {
                this.radius = radius * 1000;
                if (this.hasListeners()) {
                    this.setupQueries();
                }
            }
        }

        /**
         * Sets the center and radius (in kilometers) of this query, and triggers new events if necessary.
         * @param center The new center
         * @param radius The new radius value of this query in kilometers
         */
        public void setLocation(GeoLocation center, double radius) {
            lock (Lock) {
                this.center = center;
                // convert radius to meters
                this.radius = radius * 1000;
                if (this.hasListeners()) {
                    this.setupQueries();
                }
            }
        }
    }
}
