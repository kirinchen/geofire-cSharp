using Firebase.Database;

namespace com.surfm.firebase.geofire {
    public interface LocationCallback {

        /**
         * This method is called with the current location of the key. location will be null if there is no location
         * stored in GeoFire for the key.
         * @param key The key whose location we are getting
         * @param location The location of the key
         */
        void onLocationResult(string key, GeoLocation location);

        /**
         * Called if the callback could not be added due to failure on the server or security rules.
         * @param databaseError The error that occurred
         */
        //TODO          DatabaseError cannot create instanr
        void onCancelled(ExDatabaseError databaseError);

    }
}


