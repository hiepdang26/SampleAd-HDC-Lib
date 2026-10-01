using System;
using System.Globalization;
using System.Linq;
using CountryRegionCheck;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class YourCountry : MonoBehaviour
    {
        public string[] ignores;
        public string overrideCountry;
        
        public DropdownPro dropdownPro;
        public Button button;
        
        public TMP_Text label;

        private void Awake()
        {
            var countryName = DeviceCountry.GetName();
            if (!ignores.Contains(countryName)) overrideCountry = countryName;
            label.text = overrideCountry;

            button.onClick.AddListener(OnClickSelected);
        }

        private void OnClickSelected()
        {
            dropdownPro.ChangeValue(overrideCountry);
        }

        public static class DeviceCountry
        {
            public static string GetIso2()
            {
                string iso = AndroidTelephonyUtils.GetSimCountryIso();
                if (string.IsNullOrEmpty(iso)) iso = AndroidTelephonyUtils.GetNetworkCountryIso();
                if (string.IsNullOrEmpty(iso)) iso = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                return iso.ToUpperInvariant();
            }

            public static string GetName()
            {
                try { return new RegionInfo(GetIso2()).EnglishName; }
                catch { return GetIso2(); }
            }
        }
    }
}