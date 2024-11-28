using UnityEngine;
using com.unity3d.mediation;

namespace RetroCode
{
    public class LevelPlayManager : MonoBehaviour
    {
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

        public void LoadRewardedAd(int adTypeIndex)
        {
            //IronSource.Agent.loadRewardedVideo();

            ShowRewardedAd();
        }

        public void ShowRewardedAd()
        {
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                print("No rewarded ads available at the moment.");
            }
        }

        private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {

        }

        private void RewardedVideoOnAdUnavailable()
        {

        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {

        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {

        }

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {

        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {

        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {

        }        
    }
    
    public enum RewardedAdType
    {
        DoubleRewards,
        Revive,
        RewardedVideo,
    }
}
