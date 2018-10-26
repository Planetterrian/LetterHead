using System;
using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AdManager : Singleton<AdManager>
{

#if UNITY_ANDROID
        string bannerAdUnitId = "ca-app-pub-7112330326407860/5797002235";
    private string appId = "ca-app-pub-7112330326407860~4320269035";
#elif UNITY_IPHONE
        string bannerAdUnitId = "ca-app-pub-7112330326407860/7413336235";
    private string appId = "ca-app-pub-7112330326407860~5936603036";
#else
    string bannerAdUnitId = "unexpected_platform";
    private string appId = "unexpected_platform";
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
    private bool hasBanner;
    private bool canShowBanner;

    public bool AdsEnabled()
    {
        if (ClientManager.Instance.myUserInfo == null)
            return true;

        return !ClientManager.Instance.IsPremium();
    }

    // Use this for initialization
    void Start ()
    {
        if (AdsEnabled())
        {
            MobileAds.Initialize(appId);
            LoadInterstitial();
        }

        LoadRewardedVideo();
    }

    private void LoadRewardedVideo()
    {

    }

    private void RewardBasedVideoOnOnAdRewarded(ShowResult result)
    {
        Debug.Log("Rewarded video completed");
        onRewardAdCompleted(result);
    }

    public bool HasRewardedVideo()
    {
        return Advertisement.IsReady("rewardedVideo");
    }

    public void ShowRewardedVideo(Action<ShowResult> onCompleted)
    {
        onRewardAdCompleted = onCompleted;

        Advertisement.Show("rewardedVideo", new ShowOptions()
                                            {
                                                resultCallback = RewardBasedVideoOnOnAdRewarded
        });
    }

    internal bool ShouldShowBanner()
    {
        return SceneManager.GetActiveScene().name == "menu";
    }

    private void LoadInterstitial()
    {
        interstitial = new InterstitialAd(interstitialAdUnitId);
        AdRequest request = new AdRequest.Builder().Build();
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
            LoadInterstitial();
        };
    }

    public void ShowInterstitial()
    {
        Debug.Log("Attempting to show interstitial");

        if (!AdsEnabled())
            return;

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
        RequestBanner();
    }

    private void RequestBanner()
    {
        if (bannerView == null)
        {
            ShowBanner();
            return;
        }

        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }

    public void EnableAds()
    {
        if(adShown)
            return;

        canShowBanner = true;

        if (hasBanner)
        {
            bannerView.Show();
            BannerViewOnOnAdLoaded(null, null);
        }
        else
        {
            RequestBanner();
        }
    }

    public void DisableAds()
    {
        adShown = false;
        canShowBanner = false;

        if (bannerView != null)
        {
            bannerView.Hide();
        }

        OnBannerAdHidden.Invoke();
    }

    private void BannerViewOnOnAdLoaded(object sender, EventArgs eventArgs)
    {
        if(!AdsEnabled())
            return;

        if(!canShowBanner && bannerView != null)
        {
            bannerView.Hide();
            return;
        }

        adShown = true;
        hasBanner = true;

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
