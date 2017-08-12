using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace com.surfm.firebase.geofire.util {
    public class Base32Utils  {
        /* number of bits per base 32 character */
        public static readonly int BITS_PER_BASE32_CHAR = 5;

        private static readonly string BASE32_CHARS = "0123456789bcdefghjkmnpqrstuvwxyz";

    private Base32Utils() { }

        public static char valueToBase32Char(int value) {
            if (value < 0 || value >= BASE32_CHARS.Length) {
                throw new System.Exception("Not a valid base32 value: " + value);
            }
            return BASE32_CHARS[(value)];
        }

        public static int base32CharToValue(char base32Char) {
            int value = BASE32_CHARS.IndexOf(base32Char);
            if (value == -1) {
                throw new System.Exception("Not a valid base32 char: " + base32Char);
            } else {
                return value;
            }
        }

        public static bool isValidBase32string(string s) {
            Regex rgx = new Regex("^[" + BASE32_CHARS + "]*$");
            return rgx.Matches(s).Count > 0;
        }
    }
}


