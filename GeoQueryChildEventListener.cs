using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {
    public class GeoQueryChildEventListener {
        private GeoQuery me;

        public GeoQueryChildEventListener(GeoQuery gq) {
            me = gq;
        }

        private void onChildAdded(object sender, ChildChangedEventArgs e) {
            if (e.DatabaseError != null) {
                onCancelled(e.DatabaseError);
            } else {
                lock (me.Lock) {
                    me.childAdded(e.Snapshot);
                }
            }
        }


        private void onChildChanged(object sender, ChildChangedEventArgs e) {
            if (e.DatabaseError != null) {
                onCancelled(e.DatabaseError);
            } else {
                lock (me.Lock) {
                    me.childChanged(e.Snapshot);
                }
            }
        }
        private void onChildRemoved(object sender, ChildChangedEventArgs e) {
            if (e.DatabaseError != null) {
                onCancelled(e.DatabaseError);
            } else {
                lock (me.Lock) {
                    me.childRemoved(e.Snapshot);
                }
            }
        }


        public void onCancelled(DatabaseError databaseError) {
        }

        internal void removeEventListener(Query q) {
            q.ChildAdded -= onChildAdded;
            q.ChildChanged -= onChildChanged;
            q.ChildRemoved -= onChildRemoved;
        }

        internal void addChildEventListener(Query firebaseQuery) {
            firebaseQuery.ChildAdded += onChildAdded;
            firebaseQuery.ChildChanged += onChildChanged;
            firebaseQuery.ChildRemoved += onChildRemoved;
        }
    }
}
