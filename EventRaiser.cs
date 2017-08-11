using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.surfm.firebase.geofire {
    public interface EventRaiser {
        void raiseEvent(Action r);
    }

}


