using UnityEngine;
using com.unity3d.mediation;
using UnityEngine.Events;

namespace RetroCode
{
    public class LevelPlayManager : MonoBehaviour
    {
        public UnityEvent DoubleRewarded;
        [Space]
        public UnityEvent ReviveRewarded;
        [Space]
        public UnityEvent VideoRewarded;

        private void Start()
        {
            // ANDROID APPKEY - 1d1c92815 //

            // ANDROID //
            string appKey = "85460dcd";
            // IOS //
            //string appKey = "???";

            IronSource.Agent.validateIntegration();
            LevelPlay.Init(appKey, adFormats: new[] { LevelPlayAdFormat.REWARDED });

            LevelPlay.OnInitSuccess += SDKInitComplete;
            LevelPlay.OnInitFailed += SDKInitFailed;
        }

        private void OnApplicationPause(bool pause)
        {
            IronSource.Agent.onApplicationPause(pause);
        }

        private void SDKInitComplete(LevelPlayConfiguration config)
        {            
            print("Ads SDK Initialized.");
            EnableAds();
        }

        private void SDKInitFailed(LevelPlayInitError error)
        {
            print($"Ads SDK Failed: {error}");
        }

        private void EnableAds()
        {
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
        }

        public void LoadRewardedAd(int adTypeIndex)
        {
            //IronSource.Agent.loadRewardedVideo();

            // ADTYPEINDEXES EXPLAINED: //
            // 0 = DOUBLE REWARDS AD //
            // 1 = REVIVE AD //
            // 2 = AD FOR EXTRA LOOT //

            if (IronSource.Agent.isRewardedVideoAvailable()) IronSource.Agent.showRewardedVideo();
        }

        // CALLED AS SOON AS THERE IS AN AD AVAILABLE //
        private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
            print("Ad available, loading...");
        }

        // CALLED WHEN THERE IS NO AD AVAILABLE //
        private void RewardedVideoOnAdUnavailable()
        {
            print("No rewarded ads available at the moment.");
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            print("Ad opened.");
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            print("Ad closed.");
        }

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            print("Ad rewards given.");
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            print("Ad failed.");
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            print("Ad clicked.");
        }        
    }
    
    public enum RewardedAdType
    {
        // ADTYPEINDEXES EXPLAINED: //
        // 0 = DOUBLE REWARDS AD //
        // 1 = REVIVE AD //
        // 2 = AD FOR EXTRA LOOT //

        DoubleRewards,
        Revive,
        RewardedVideo,
    }
}
