using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.surfm.firebase.geofire {

    public class UnityEventRaiser : MonoBehaviour, EventRaiser {
        public void raiseEvent(Action r) {
            r();
        }
    }
}