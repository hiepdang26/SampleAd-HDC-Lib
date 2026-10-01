using System.Collections;
using System.Collections.Generic;
using BG_Library.NET.API;
using UnityEngine;

public class LoadAd : MonoBehaviour
{
    
   public void loadAd()
    {
        NetCallerAPI.AR_InitManually();
        NetCallerAPI.FA_InitManually("native_gameplay");
        NetCallerAPI.FA_InitManually("native_gameplay");
    }
}
