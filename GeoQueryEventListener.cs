using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.surfm.firebase.geofire {
    public interface GeoQueryEventListener {

        /**
         * Called if a key entered the search area of the GeoQuery. This method is called for every key currently in the
         * search area at the time of adding the listener.
         *
         * This method is once per key, and is only called again if onKeyExited was called in the meantime.
         *
         * @param key The key that entered the search area
         * @param location The location for this key as a GeoLocation object
         */
         void onKeyEntered(string key, GeoLocation location);

        /**
         * Called if a key exited the search area of the GeoQuery. This is method is only called if onKeyEntered was called
         * for the key.
         *
         * @param key The key that exited the search area
         */
         void onKeyExited(string key);

        /**
         * Called if a key moved within the search area.
         *
         * This method can be called multiple times.
         *
         * @param key The key that moved within the search area
         * @param location The location for this key as a GeoLocation object
         */
         void onKeyMoved(string key, GeoLocation location);

        /**
         * Called once all initial GeoFire data has been loaded and the relevant events have been fired for this query.
         * Every time the query criteria is updated, this observer will be called after the updated query has fired the
         * appropriate key entered or key exited events.
         */
         void onGeoQueryReady();

        /**
         * Called in case an error occurred while retrieving locations for a query, e.g. violating security rules.
         * @param error The error that occurred while retrieving the query
         */
         void onGeoQueryError(DatabaseError error);

    }
}


