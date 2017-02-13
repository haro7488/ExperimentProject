using UnityEngine;
using System.Collections;

namespace EasyMobile
{
    [System.Serializable]
    public class AdSettings
    {
        public AdMobConfig IosAdMobConfig { get { return _iosAdMobConfig; } }

        public AdMobConfig AndroidAdMobConfig { get { return _androidAdMobConfig; } }

        public string HeyzapPublisherId { get { return _heyzapPublisherId; } }

        public bool HeyzapShowTestSuite { get { return _heyzapShowTestSuite; } }

        public bool IsAutoLoadDefaultAds { get { return _autoLoadDefaultAds; } set { _autoLoadDefaultAds = value; } }

        public float AdCheckingInterval { get { return _adCheckingInterval; } set { _adCheckingInterval = value; } }

        public float AdLoadingInterval { get { return _adLoadingInterval; } set { _adLoadingInterval = value; } }

        public DefaultAdNetworks IosDefaultAdNetworks { get { return _iosDefaultAdNetworks; } }

        public DefaultAdNetworks AndroidDefaultAdNetworks { get { return _androidDefaultAdNetwork; } }

        // AdMob config
        [SerializeField]
        private AdMobConfig _iosAdMobConfig;
        [SerializeField]
        private AdMobConfig _androidAdMobConfig;

        // Heyzap config
        [SerializeField]
        private string _heyzapPublisherId;
        [SerializeField]
        private bool _heyzapShowTestSuite;

        [SerializeField]
        private bool _autoLoadDefaultAds = true;
        [SerializeField]
        private float _adCheckingInterval = 10f;
        [SerializeField]
        private float _adLoadingInterval = 20f;

        [SerializeField]
        private DefaultAdNetworks _iosDefaultAdNetworks = new DefaultAdNetworks(BannerAdNetwork.None, InterstitialAdNetwork.None, RewardedAdNetwork.None);
        [SerializeField]
        private DefaultAdNetworks _androidDefaultAdNetwork = new DefaultAdNetworks(BannerAdNetwork.None, InterstitialAdNetwork.None, RewardedAdNetwork.None);

        [System.Serializable]
        public struct DefaultAdNetworks
        {
            public BannerAdNetwork bannerAdNetwork;
            public InterstitialAdNetwork interstitialAdNetwork;
            public RewardedAdNetwork rewardedAdNetwork;

            public DefaultAdNetworks(BannerAdNetwork banner, InterstitialAdNetwork interstitial, RewardedAdNetwork rewarded)
            {
                bannerAdNetwork = banner;
                interstitialAdNetwork = interstitial;
                rewardedAdNetwork = rewarded;
            }
        }

        [System.Serializable]
        public struct AdMobConfig
        {
            public string bannerAdId;
            public string interstitialAdId;
            // AdMob rewarded ads are not officially supported by us now.
            [HideInInspector]
            public string rewardedAdId;
        }
    }

    // List of all supported ad networks
    public enum AdNetwork
    {
        None,
        AdMob,
        Chartboost,
        Heyzap,
        UnityAds
    }

    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded
    }

    public enum BannerAdNetwork
    {
        None = AdNetwork.None,
        AdMob = AdNetwork.AdMob,
        Heyzap = AdNetwork.Heyzap
    }

    public enum InterstitialAdNetwork
    {
        None = AdNetwork.None,
        AdMob = AdNetwork.AdMob,
        Chartboost = AdNetwork.Chartboost,
        Heyzap = AdNetwork.Heyzap,
        UnityAds = AdNetwork.UnityAds
    }

    public enum RewardedAdNetwork
    {
        None = AdNetwork.None,
        Chartboost = AdNetwork.Chartboost,
        Heyzap = AdNetwork.Heyzap,
        UnityAds = AdNetwork.UnityAds
    }
}

