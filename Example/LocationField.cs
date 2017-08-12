using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.surfm.firebase.geofire.test {
    public class LocationField : MonoBehaviour {

        public float speed = 0.1f;
        private Toggle toggle;
        private InputField inputField;


        void Awake() {
            toggle = GetComponentInChildren<Toggle>();
            inputField = GetComponentInChildren<InputField>();
        }

        // Update is called once per frame
        void Update() {
            if (!toggle.isOn) return;
            float x = Input.GetAxis("Vertical") * speed;
            float y = Input.GetAxis("Horizontal") * speed;
            if (Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0) {
                GeoLocation gl = getLocation();
                inputField.text = Math.Round(gl.latitude + x, 6) + "," + Math.Round(gl.longitude + y, 6);
            }
        }

        public GeoLocation getLocation() {
            string[] ta = inputField.text.Split(',');
            return new GeoLocation(double.Parse(ta[0]), double.Parse(ta[1]));
        }
    }
}
