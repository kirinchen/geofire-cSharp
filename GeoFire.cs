using com.surfm.firebase.geofire.core;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.surfm.firebase.geofire {


    /**
     * A GeoFire instance is used to store geo location data in Firebase.
     */
    public class GeoFire {

        /**
         * A listener that can be used to be notified about a successful write or an error on writing.
         */
        public interface CompletionListener {
            /**
             * Called once a location was successfully saved on the server or an error occurred. On success, the parameter
             * error will be null; in case of an error, the error will be passed to this method.
             *
             * @param key   The key whose location was saved
             * @param error The error or null if no error occurred
             */
            void onComplete(string key, DatabaseError error);
        }

        /**
         * A small wrapper class to forward any events to the LocationEventListener.
         */
        public class LocationValueEventListener {

            private LocationCallback callback;
            private DatabaseReference keyRef;

            public LocationValueEventListener(LocationCallback callback, DatabaseReference keyRef) {
                this.callback = callback;
                this.keyRef = keyRef;
                keyRef.ValueChanged += (this.onDataChange);
                //System.EventHandler<ValueChangedEventArgs> ValueChanged = onDataChange;
            }

            public void onDataChange(object sender, ValueChangedEventArgs e) {
                keyRef.ValueChanged -= (this.onDataChange);
                if (e.DatabaseError != null) {
                    onCancelled(e.DatabaseError);
                } else {
                    onDataChange(e.Snapshot);
                }
            }

            public void onDataChange(DataSnapshot dataSnapshot) {
                if (dataSnapshot.Value == null) {
                    this.callback.onLocationResult(dataSnapshot.Key, null);
                } else {
                    GeoLocation location = GeoFire.getLocationValue(dataSnapshot);
                    if (location != null) {
                        this.callback.onLocationResult(dataSnapshot.Key, location);
                    } else {
                        string message = "GeoFire data has invalid format: " + dataSnapshot.Value;
                        this.callback.onCancelled(new ExDatabaseError(new Exception(message)));
                    }
                }
            }

            public void onCancelled(DatabaseError databaseError) {
                this.callback.onCancelled(new ExDatabaseError(databaseError));
            }
        }

        public static GeoLocation getLocationValue(DataSnapshot dataSnapshot) {
            try {
                return GeoLocationConverter.getInstance().parse(dataSnapshot.Value);

            } catch (NullReferenceException e) {
                return null;
            } catch (InvalidCastException e) {
                return null;
            }
        }

        private readonly DatabaseReference databaseReference;
        private readonly EventRaiser eventRaiser;

        /**
         * Creates a new GeoFire instance at the given Firebase reference.
         *
         * @param databaseReference The Firebase reference this GeoFire instance uses
         */
        public GeoFire(DatabaseReference databaseReference) {
            this.databaseReference = databaseReference;
            this.eventRaiser = new BaseEventRaiser();
        }

        /**
         * @return The Firebase reference this GeoFire instance uses
         */
        public DatabaseReference getDatabaseReference() {
            return this.databaseReference;
        }

        public DatabaseReference getDatabaseRefForKey(string key) {
            return this.databaseReference.Child(key);
        }

        /**
         * Sets the location for a given key.
         *
         * @param key      The key to save the location for
         * @param location The location of this key
         */
        public void setLocation(string key, GeoLocation location) {
            if (key == null) {
                throw new NullReferenceException();
            }
            DatabaseReference keyRef = this.getDatabaseRefForKey(key);
            GeoHash geoHash = new GeoHash(location);
            //Dictionary<string, object> updates = new Dictionary<string, object>();
            //updates.Add("g", geoHash.getGeoHashString());
            //updates.Add("l", new List<object>(new object[] { location.latitude, location.longitude }));
            keyRef.SetValueAsync(GeoLocationConverter.getInstance().toFireObj(geoHash.getGeoHashString(), location), geoHash.getGeoHashString());
        }



        /**
         * Removes the location for a key from this GeoFire.
         *
         * @param key The key to remove from this GeoFire
         */
        public void removeLocation(string key) {
            if (key == null) {
                throw new NullReferenceException();
            }
            DatabaseReference keyRef = this.getDatabaseRefForKey(key);

            keyRef.SetValueAsync(null);
        }



        /**
         * Gets the current location for a key and calls the callback with the current value.
         *
         * @param key      The key whose location to get
         * @param callback The callback that is called once the location is retrieved
         */
        public void getLocation(string key, LocationCallback callback) {
            DatabaseReference keyRef = this.getDatabaseRefForKey(key);
            LocationValueEventListener valueListener = new LocationValueEventListener(callback, keyRef);
            
        }

        /**
         * Returns a new Query object centered at the given location and with the given radius.
         *
         * @param center The center of the query
         * @param radius The radius of the query, in kilometers
         * @return The new GeoQuery object
         */
        public GeoQuery queryAtLocation(GeoLocation center, double radius) {
            return new GeoQuery(this, center, radius);
        }

        public void raiseEvent(Action r) {
            this.eventRaiser.raiseEvent(r);
        }
    }

}

