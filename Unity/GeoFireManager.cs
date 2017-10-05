using Firebase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Unity.Editor;
using Firebase.Database;

namespace com.surfm.firebase.geofire {
    public abstract class GeoFireManager : MonoBehaviour, GeoQueryEventListener {

        public string geoFireUrl;
        protected GeoFire geoFire;
        private GeoQuery query;
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        private List<Action> readyActions = new List<Action>();
        public void addReadyAction(Action a) { if (readyActions == null) { a(); } else { readyActions.Add(a); } }
        public bool readyed { get; private set; }
        private Func<GeoLocation> centerFunc;

        public void init(Func<GeoLocation> geoFunc, Action readyAction) {
            if (geoFunc == null) throw new NullReferenceException("geoFunc is empty");
            centerFunc = geoFunc;
            initFirebase();
            addReadyAction(readyAction);
            onCenterLocationChange();

        }

        public void onCenterLocationChange() {
            Debug.Log("onCenterLocationChange");
            if (query == null) {
                query = geoFire.queryAtLocation(centerFunc(), 1);
                query.addGeoQueryEventListener(this);
            } else {
                query.setCenter(centerFunc());
            }
        }

        private void initFirebase() {
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
        }

        internal virtual void initializeFirebase() {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            app.SetEditorDatabaseUrl(geoFireUrl);
            if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
            geoFire = new GeoFire(FirebaseDatabase.DefaultInstance.GetReference("/geo"));
            //  query = geoFire.queryAtLocation(ceneterLocation.getLocation(), 2);
           // query.addGeoQueryEventListener(this);
        }


        public virtual void onGeoQueryReady() {
            if (!readyed) {
                readyed = true;
                readyActions.ForEach(a => { a(); });
                readyActions = null;
            }
        }

        public abstract void onKeyEntered(string key, GeoLocation location);
        public abstract void onKeyExited(string key);
        public abstract void onKeyMoved(string key, GeoLocation location);
        public abstract void onGeoQueryError(DatabaseError error);
    }
}
