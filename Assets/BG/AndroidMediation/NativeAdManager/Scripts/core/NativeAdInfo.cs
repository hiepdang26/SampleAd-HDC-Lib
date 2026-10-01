using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NativeAdInfo
{
        public string adUnitId;

        // Core
        public string headline;
        public string body;
        public string callToAction;
        public string advertiser;
        public string store;
        public string price;

        // Mediation
        public string mediationAdapter;
        public string responseId;
        public string adSource;
        public string adSourceId;

        // Revenue (optional)
        public long? revenueMicros;
        public string currencyCode;
        public int? precisionType;
}
