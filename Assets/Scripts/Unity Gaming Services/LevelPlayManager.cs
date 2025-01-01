using UnityEngine;
using com.unity3d.mediation;
using UnityEngine.Events;

namespace RetroCode
{
    public class LevelPlayManager : MonoBehaviour
    {
        [Header("Events")]
        [Space]
        public UnityEvent AdLoaded;
        [Space]
        public UnityEvent DoubleRewarded;
        [Space]
        public UnityEvent ReviveRewarded;
        [Space]
        public UnityEvent VideoRewarded;
        [Space]
        public UnityEvent AdFailed;

        // ANDROID APPKEY - 1d1c92815 //

        // ANDROID //
#if UNITY_ANDROID
        private string appKey = "85460dcd";
        private string interstitialAdUnitId = "aeyqi3vqlv6o8sh9";
#endif
        // IOS //
#if UNITY_IPHONE
        private string appKey = "8545d445";
        private string interstitialAdUnitId = "wmgt0712uuux8ju4";
#endif

#if UNITY_EDITOR
        private string appKey = "85460dcd";
        private string interstitialAdUnitId = "aeyqi3vqlv6o8sh9";
#endif

        private LevelPlayInterstitialAd interstitialAd;

        private void Start()
        {
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

            interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
        }

        #region Rewarded Ad
        private int AdType;
        public void LoadRewardedAd(int adTypeIndex)
        {
            //IronSource.Agent.loadRewardedVideo();

            // ADTYPEINDEXES EXPLAINED: //
            // 0 = DOUBLE REWARDS AD //
            // 1 = REVIVE AD //
            // 2 = AD FOR EXTRA LOOT //

            AdType = adTypeIndex;

            if (IronSource.Agent.isRewardedVideoAvailable()) 
            {
                IronSource.Agent.showRewardedVideo();
                AdLoaded?.Invoke();
            }
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

            switch (AdType)
            {
                case 0:
                    DoubleRewarded?.Invoke();
                    break;
                case 1:
                    ReviveRewarded?.Invoke();
                    break;                    
                case 2:
                    VideoRewarded?.Invoke();
                    break;
            }
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            print("Ad failed.");

            AdFailed?.Invoke();
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            print("Ad clicked.");
        }
        #endregion

        #region Interstitial Ad
        public void LoadInterstitialAd()
        {
            interstitialAd.LoadAd();
            if (interstitialAd.IsAdReady()) interstitialAd.ShowAd();
        }

        private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            print("Interstitial loaded.");
            //if (interstitialAd.IsAdReady()) interstitialAd.ShowAd();
        }

        private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            print($"Interstitial load failed: {error}");
        }

        private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            print("Interstitial displayed.");
        }

        private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError)
        {
            print("Interstitial display failed.");
        }

        private void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            print("Interstitial clicked.");
        }

        private void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            print("Interstitial closed.");
        }

        private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            print("Interstitial info changed.");
        }
        #endregion
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
