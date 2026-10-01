using BG_Library.NET.API;
using BG_Library.NET.AdSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace AppBootstrap.Splash
{
    internal sealed class SplashFAAdHandler : ISplashAdHandler
    {
        public AdGroupType GroupType => AdGroupType.FA;
        

        public void Initialize(string groupName)
        {
            SplashLogger.Log($"FA handler init group={groupName}");
            NetCallerAPI.FA_InitManually(groupName);
        }

        public bool IsReady(string groupName)
        {
            bool isReady = AdsLogic.Ins.FA_ManagerIns.IsGroupReady(groupName);
            return isReady;
        }

        public void AdShow(AdGroupEntry groupEntry, AdInfoConfig adInfoConfig, IAdStorage adStorage, SplashAdExecutionResult result, SplashAdShowContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(result.SelectedAdsPosition))
            {
                SplashLogger.Warn($"FA handler show skipped group={result.SelectedGroupName} reason=empty-position");
                return;
            }
            
            SplashLogger.Log(
                $"FA handler show start entryGroup={groupEntry.groupName} selectedGroup={result.SelectedGroupName} " +
                $"selectedPos={result.SelectedAdsPosition} primary={result.PrimaryReady} backup={result.BackupReady}");
            adStorage.SetStatus(groupEntry.type, groupEntry.groupName, AdGroupStatus.AdsShowing);
            NetCallerAPI.FA_Show(result.SelectedAdsPosition, () =>
            {
                adStorage.SetStatus(groupEntry.type, groupEntry.groupName, AdGroupStatus.AdsComplete);
                SplashLogger.Log($"FA handler show invoked api pos={result.SelectedAdsPosition}, waiting-complete=true");
            });
        }

        public async UniTask WaitToCompleteHandlerAsync(AdGroupEntry groupEntry, IAdStorage adStorage, CancellationToken cancellationToken)
        {
            if(adStorage.GetStatus(groupEntry.type, groupEntry.groupName) != AdGroupStatus.AdsShowing) 
                return;
            
            await UniTask.WaitUntil(() => adStorage.GetStatus(groupEntry.type, groupEntry.groupName) == AdGroupStatus.AdsComplete, cancellationToken: cancellationToken);
        }
    }
}
