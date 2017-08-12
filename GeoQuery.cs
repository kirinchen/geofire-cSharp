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
        static object Lock = new object();
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


        public class ChildEventListener {

            private DatabaseReference keyRef;
            private GeoQuery me;

            public ChildEventListener(GeoQuery gq,DatabaseReference keyRef) {
                this.keyRef = keyRef;
                me = gq;
                //TODO
                keyRef.ChildRemoved += (s, e) => {
                   // onNodeChange(s, e, me.childRemoved);
                };
                //System.EventHandler<ValueChangedEventArgs> ValueChanged = onDataChange;
            }



            public void onNodeChange(object sender, ChildChangedEventArgs e, Action<DataSnapshot> da) {
                if (e.DatabaseError == null) {
                    onCancelled(e.DatabaseError);
                } else {
                    lock (Lock) {

                    }
                }
            }



            public void onCancelled(DatabaseError databaseError) {
            }
        }

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
        GeoQuery(GeoFire geoFire, GeoLocation center, double radius) {
            this.geoFire = geoFire;
            this.center = center;
            // convert from kilometers to meters
            this.radius = radius * 1000;
        }

        private bool locationIsInQuery(GeoLocation location) {
            return GeoUtils.distance(location, center) <= this.radius;
        }

        private void updateLocationInfo(string key, GeoLocation location) {
            LocationInfo oldInfo = this.locationInfos.get(key);
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
            this.locationInfos.Add(key, newInfo);
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
            for (Map.Entry<GeoHashQuery, Query> entry: this.firebaseQueries.entrySet()) {
                entry.getValue().removeEventListener(this.childEventLister);
            }
            this.outstandingQueries.clear();
            this.firebaseQueries.clear();
            this.queries = null;
            this.locationInfos.clear();
        }

        private bool hasListeners() {
            return !this.eventListeners.isEmpty();
        }

        private bool canFireReady() {
            return this.outstandingQueries.isEmpty();
        }

        private void checkAndFireReady() {
            if (canFireReady()) {
                for (readonly GeoQueryEventListener listener: this.eventListeners) {
                    this.geoFire.raiseEvent(new Runnable() {
                    @Override
                    public void run() {
            listener.onGeoQueryReady();
        }
    });
            }
        }
    }

    private void addValueToReadyListener(readonly Query firebase, readonly GeoHashQuery query) {
    firebase.addListenerForSingleValueEvent(new ValueEventListener() {
            @Override
            public void onDataChange(DataSnapshot dataSnapshot) {
    synchronized(GeoQuery.this) {
        GeoQuery.this.outstandingQueries.remove(query);
        GeoQuery.this.checkAndFireReady();
    }
}

@Override
            public void onCancelled(readonly DatabaseError databaseError) {
    synchronized(GeoQuery.this) {
        for (readonly GeoQueryEventListener listener : GeoQuery.this.eventListeners) {
            GeoQuery.this.geoFire.raiseEvent(new Runnable() {
                            @Override
                            public void run() {
    listener.onGeoQueryError(databaseError);
}
                        });
                    }
                }
            }
        });
    }

    private void setupQueries() {
    Set<GeoHashQuery> oldQueries = (this.queries == null) ? new HashSet<GeoHashQuery>() : this.queries;
    Set<GeoHashQuery> newQueries = GeoHashQuery.queriesAtLocation(center, radius);
    this.queries = newQueries;
    for (GeoHashQuery query: oldQueries) {
        if (!newQueries.contains(query)) {
            firebaseQueries.get(query).removeEventListener(this.childEventLister);
            firebaseQueries.remove(query);
            outstandingQueries.remove(query);
        }
    }
    for (readonly GeoHashQuery query: newQueries) {
        if (!oldQueries.contains(query)) {
            outstandingQueries.add(query);
            DatabaseReference databaseReference = this.geoFire.getDatabaseReference();
            Query firebaseQuery = databaseReference.orderByChild("g").startAt(query.getStartValue()).endAt(query.getEndValue());
            firebaseQuery.addChildEventListener(this.childEventLister);
            addValueToReadyListener(firebaseQuery, query);
            firebaseQueries.put(query, firebaseQuery);
        }
    }
    for (Map.Entry<string, LocationInfo> info: this.locationInfos.entrySet()) {
        LocationInfo oldLocationInfo = info.getValue();
        this.updateLocationInfo(info.getKey(), oldLocationInfo.location);
    }
    // remove locations that are not part of the geo query anymore
    Iterator<Map.Entry<string, LocationInfo>> it = this.locationInfos.entrySet().iterator();
    while (it.hasNext()) {
        Map.Entry<string, LocationInfo> entry = it.next();
        if (!this.geoHashQueriesContainGeoHash(entry.getValue().geoHash)) {
            it.remove();
        }
    }

    checkAndFireReady();
}

private void childAdded(DataSnapshot dataSnapshot) {
    GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
    if (location != null) {
        this.updateLocationInfo(dataSnapshot.Key, location);
    } else {
        // throw an error in future?
    }
}

private void childChanged(DataSnapshot dataSnapshot) {
    GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
    if (location != null) {
        this.updateLocationInfo(dataSnapshot.getKey(), location);
    } else {
        // throw an error in future?
    }
}

private void childRemoved(DataSnapshot dataSnapshot) {
    string key = dataSnapshot.Key;
    LocationInfo info = this.locationInfos[key];
    if (info != null) {
        this.geoFire.getDatabaseRefForKey(key).addListenerForSingleValueEvent(new ValueEventListener() {
                @Override
                public void onDataChange(DataSnapshot dataSnapshot) {
    synchronized(GeoQuery.this) {
        GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
        GeoHash hash = (location != null) ? new GeoHash(location) : null;
        if (hash == null || !GeoQuery.this.geoHashQueriesContainGeoHash(hash)) {
            readonly LocationInfo info = GeoQuery.this.locationInfos.get(key);
            GeoQuery.this.locationInfos.remove(key);
            if (info != null && info.inGeoQuery) {
                for (readonly GeoQueryEventListener listener: GeoQuery.this.eventListeners) {
                    GeoQuery.this.geoFire.raiseEvent(new Runnable() {
                                        @Override
                                        public void run() {
    listener.onKeyExited(key);
}
    });
                                }
                            }
                        }
                    }
                }

                @Override
                public void onCancelled(DatabaseError databaseError) {
    // tough luck
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
    public synchronized void addGeoQueryEventListener(readonly GeoQueryEventListener listener) {
    if (eventListeners.contains(listener)) {
        throw new IllegalArgumentException("Added the same listener twice to a GeoQuery!");
    }
    eventListeners.add(listener);
    if (this.queries == null) {
        this.setupQueries();
    } else {
        for (readonly Map.Entry<string, LocationInfo> entry: this.locationInfos.entrySet()) {
            readonly string key = entry.getKey();
            readonly LocationInfo info = entry.getValue();
            if (info.inGeoQuery) {
                this.geoFire.raiseEvent(new Runnable() {
                        @Override
                        public void run() {
    listener.onKeyEntered(key, info.location);
}
                    });
                }
            }
            if (this.canFireReady()) {
    this.geoFire.raiseEvent(new Runnable() {
                    @Override
                    public void run() {
    listener.onGeoQueryReady();
}
                });
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
    public synchronized void removeGeoQueryEventListener(GeoQueryEventListener listener) {
    if (!eventListeners.contains(listener)) {
        throw new IllegalArgumentException("Trying to remove listener that was removed or not added!");
    }
    eventListeners.remove(listener);
    if (!this.hasListeners()) {
        reset();
    }
}

/**
 * Removes all event listeners from this GeoQuery.
 */
public synchronized void removeAllListeners() {
    eventListeners.clear();
    reset();
}

/**
 * Returns the current center of this query.
 * @return The current center
 */
public synchronized GeoLocation getCenter() {
    return center;
}

/**
 * Sets the new center of this query and triggers new events if necessary.
 * @param center The new center
 */
public synchronized void setCenter(GeoLocation center) {
    this.center = center;
    if (this.hasListeners()) {
        this.setupQueries();
    }
}

/**
 * Returns the radius of the query, in kilometers.
 * @return The radius of this query, in kilometers
 */
public synchronized double getRadius() {
    // convert from meters
    return radius / 1000;
}

/**
 * Sets the radius of this query, in kilometers, and triggers new events if necessary.
 * @param radius The new radius value of this query in kilometers
 */
public synchronized void setRadius(double radius) {
    // convert to meters
    this.radius = radius * 1000;
    if (this.hasListeners()) {
        this.setupQueries();
    }
}

/**
 * Sets the center and radius (in kilometers) of this query, and triggers new events if necessary.
 * @param center The new center
 * @param radius The new radius value of this query in kilometers
 */
public synchronized void setLocation(GeoLocation center, double radius) {
    this.center = center;
    // convert radius to meters
    this.radius = radius * 1000;
    if (this.hasListeners()) {
        this.setupQueries();
    }
}
}
}
