using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {
    public class ExDatabaseError  {

        public DatabaseError originalError { get; private set; }
        public Exception otherError { get; private set; }

        public ExDatabaseError(DatabaseError originalError) {
            this.originalError = originalError;
        }

        public ExDatabaseError(Exception e) {
            otherError = e;
        }


    }
}

