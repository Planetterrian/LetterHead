using System;
using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class AdManager : Singleton<AdManager>
{

#if UNITY_ANDROID
        string bannerAdUnitId = "ca-app-pub-7112330326407860/5797002235";
#elif UNITY_IPHONE
        string bannerAdUnitId = "ca-app-pub-7112330326407860/7413336235";
#else
    string bannerAdUnitId = "unexpected_platform";
#endif

#if UNITY_ANDROID
    string interstitialAdUnitId = "ca-app-pub-7112330326407860/7273735432";
#elif UNITY_IPHONE
        string interstitialAdUnitId = "ca-app-pub-7112330326407860/8890069430";
#else
        string interstitialAdUnitId = "unexpected_platform";
#endif

#if UNITY_ANDROID
    string rewardedAdUnitId = "ca-app-pub-7112330326407860/8511306234";
#elif UNITY_IPHONE
        string rewardedAdUnitId = "ca-app-pub-7112330326407860/5557839830";
#else
        string rewardedAdUnitId = "unexpected_platform";
#endif


    private BannerView bannerView;

    public UnityEvent OnBannerAdShown;
    public UnityEvent OnBannerAdHidden;
    private bool adShown;
    private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardBasedVideo;
    private Action<ShowResult> onRewardAdCompleted;

    public bool AdsEnabled()
    {
        if (ClientManager.Instance.myUserInfo == null)
            return true;

        return !ClientManager.Instance.myUserInfo.IsPremium;
    }

    // Use this for initialization
    void Start ()
    {
        if (AdsEnabled())
        {
            ShowBanner();
            LoadInterstitial();
        }

        LoadRewardedVideo();
    }

    private void LoadRewardedVideo()
    {
/*        rewardBasedVideo = RewardBasedVideoAd.Instance;

        rewardBasedVideo.OnAdRewarded += RewardBasedVideoOnOnAdRewarded;
        rewardBasedVideo.OnAdClosed += (sender, args) =>
        {
            Debug.Log("Rewarded ad closed");
        };

        rewardBasedVideo.OnAdLoaded += (sender, args) =>
        {
            Debug.Log("Rewarded ad loaded");
        };

        rewardBasedVideo.OnAdFailedToLoad += (sender, args) =>
        {
            Debug.Log("Failed to load rewarded ad. " + args.Message);
        };

        AdRequest request = new AdRequest.Builder().Build();
        request.TestDevices.Add("96f0159b9494d5c6174f95f199b659bc");
        rewardBasedVideo.LoadAd(request, rewardedAdUnitId);*/
    }

    private void RewardBasedVideoOnOnAdRewarded(ShowResult result)
    {
        Debug.Log("Rewarded video completed");
        onRewardAdCompleted(result);
/*
        AdRequest request = new AdRequest.Builder().Build();
        rewardBasedVideo.LoadAd(request, rewardedAdUnitId);*/
    }

    public bool HasRewardedVideo()
    {
        //return rewardBasedVideo.IsLoaded();
        return Advertisement.IsReady("rewardedVideo");
    }

    public void ShowRewardedVideo(Action<ShowResult> onCompleted)
    {
        onRewardAdCompleted = onCompleted;

        Advertisement.Show("rewardedVideo", new ShowOptions()
                                            {
                                                resultCallback = RewardBasedVideoOnOnAdRewarded
        });
        //rewardBasedVideo.Show();
    }

    private void LoadInterstitial()
    {
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(interstitialAdUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        request.TestDevices.Add("96f0159b9494d5c6174f95f199b659bc");
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
        interstitial.OnAdLoaded += (sender, args) =>
        {
            Debug.Log("Interstitial Loaded");
        };

        interstitial.OnAdFailedToLoad += (sender, args) =>
        {
            Debug.Log("Interstial failed to load: " + args.Message);
        };

        interstitial.OnAdClosed += (sender, args) =>
        {
            AdRequest request2 = new AdRequest.Builder().Build();
            // Load the interstitial with the request.
            interstitial.LoadAd(request2);
        };
    }

    public void ShowInterstitial()
    {
        if(!AdsEnabled())
            return;

        Debug.Log("Attempting to show interstitial");

        if(interstitial == null)
            return;

        if (interstitial.IsLoaded())
        {
            interstitial.Show();
        }
    }

    void ShowBanner()
    {
        Debug.Log("Showing banner ad");
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(bannerAdUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        bannerView.OnAdLoaded += BannerViewOnOnAdLoaded;
        bannerView.OnAdFailedToLoad += (sender, args) =>
        {
            Debug.Log("Error loading banner: " + args.Message);
        };

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        request.TestDevices.Add("96f0159b9494d5c6174f95f199b659bc");
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    public void DisableAds()
    {
        adShown = false;
        bannerView.Hide();
        OnBannerAdHidden.Invoke();
    }

    private void BannerViewOnOnAdLoaded(object sender, EventArgs eventArgs)
    {
        adShown = true;

        if(OnBannerAdShown != null)
            OnBannerAdShown.Invoke();
    }

    // Update is called once per frame
	void Update () {
	
	}

    public bool BannerShown()
    {
        return adShown;
    }
}
