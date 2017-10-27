using Firebase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Unity.Editor;

namespace com.surfm.firebase.geofire.test {
    public class TestManager : GeoFireManager {

        public LocationField putLoacation;
        public LocationField ceneterLocation;
        //private GeoFire geoFire;
        //private GeoQuery query;
        //DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        void Start() {
            init("",ceneterLocation.getLocation, () => {
                Debug.Log("TestManager inited");
                ceneterLocation.onChangeAction = onCenterChange;
            });
        }

        private void onCenterChange() {
            onCenterLocationChange();
        }

        public void putPoint() {
            geoFire.setLocation("t" + Mathf.RoundToInt(Time.time * 1000), putLoacation.getLocation());
        }

        public override void onKeyEntered(string key, GeoLocation location) {
            Debug.Log(string.Format("onKeyEntered key={0} loc={1}", key, location));
        }

        public override void onKeyExited(string key) {
            Debug.Log(string.Format("onKeyExited key={0} ", key));
        }

        public override void onKeyMoved(string key, GeoLocation location) {
            Debug.Log(string.Format("onKeyMoved key={0} loc={1}", key, location));
        }

        public override void onGeoQueryReady() {
            base.onGeoQueryReady();
            Debug.Log("onGeoQueryReady");
        }

        public override void onGeoQueryError(DatabaseError error) {
            Debug.Log("onGeoQueryError=" + error);
        }
    }
}
