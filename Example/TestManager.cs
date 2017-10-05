using Firebase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Unity.Editor;

namespace com.surfm.firebase.geofire.test {
    public class TestManager : MonoBehaviour, GeoQueryEventListener {

        public LocationField putLoacation;
        public LocationField ceneterLocation;
        private GeoFire geoFire;
        private GeoQuery query;
        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        void Start() {
            dependencyStatus = FirebaseApp.CheckDependencies();
            if (dependencyStatus != DependencyStatus.Available) {
                FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
                    dependencyStatus = FirebaseApp.CheckDependencies();
                    if (dependencyStatus == DependencyStatus.Available) {
                        initializeFirebase();
                    } else {
                        Debug.LogError(
                            "Could not resolve all Firebase dependencies: " + dependencyStatus);
                    }
                });
            } else {
                initializeFirebase();
            }
            ceneterLocation.onChangeAction = onCenterChange;
        }

        private void onCenterChange() {
            if (query != null) {
                query.setCenter(ceneterLocation.getLocation());
            }
        }

        private void initializeFirebase() {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            app.SetEditorDatabaseUrl("https://apphi-224fb.firebaseio.com/");
            if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
            geoFire = new GeoFire(FirebaseDatabase.DefaultInstance.GetReference("/geo"));
            query = geoFire.queryAtLocation(ceneterLocation.getLocation(), 200);
            query.addGeoQueryEventListener(this);
        }



        public void putPoint() {
            geoFire.setLocation("t" + Mathf.RoundToInt(Time.time * 1000), putLoacation.getLocation());
        }

        public void onKeyEntered(string key, GeoLocation location) {
            Debug.Log(string.Format("onKeyEntered key={0} loc={1}", key, location));
        }

        public void onKeyExited(string key) {
            Debug.Log(string.Format("onKeyExited key={0} ", key));
        }

        public void onKeyMoved(string key, GeoLocation location) {
            Debug.Log(string.Format("onKeyMoved key={0} loc={1}", key, location));
        }

        public void onGeoQueryReady() {
            Debug.Log("onGeoQueryReady");
        }

        public void onGeoQueryError(DatabaseError error) {
            Debug.Log("onGeoQueryError=" + error);
        }
    }
}
