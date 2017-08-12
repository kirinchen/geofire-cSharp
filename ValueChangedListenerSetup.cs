using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {
    public class ValueChangedListenerSetup {

        private Action<ValueChangedEventArgs> callback;
        private Query query;
        public bool singleble { get; private set; }
        public ValueChangedListenerSetup(Query q,bool sb, Action<ValueChangedEventArgs> callback) {
            this.callback = callback;
            query = q;
            singleble = sb;
            query.ValueChanged += onChange;
        }

        public void setSingleble(bool b) {
            singleble = b;
        }

        private void onChange(object sender, ValueChangedEventArgs e) {
            if (singleble) {
                query.ValueChanged -= onChange;
            }
            callback(e);
        }


    }
}
