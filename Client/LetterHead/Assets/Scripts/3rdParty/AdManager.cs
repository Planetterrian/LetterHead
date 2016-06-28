using System;
using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
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


    private BannerView bannerView;

    public UnityEvent OnBannerAdShown;
    public UnityEvent OnBannerAdHidden;
    private bool adShown;
    private InterstitialAd interstitial;

    public bool AdsEnabled()
    {
        return PlayerPrefs.GetInt("PurchaseMade", 0) == 0;
    }

    // Use this for initialization
    void Start ()
    {
        if (AdsEnabled())
        {
            ShowBanner();
            LoadInterstitial();
        }
    }

    private void LoadInterstitial()
    {
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(interstitialAdUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
        interstitial.OnAdLoaded += (sender, args) =>
        {
            Debug.Log("Interstitial Loaded");
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
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        bannerView.OnAdLoaded += BannerViewOnOnAdLoaded;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
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
