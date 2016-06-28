using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour {

#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-7112330326407860/5797002235";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-7112330326407860/7413336235";
#else
    string adUnitId = "unexpected_platform";
#endif

    // Use this for initialization
    void Start ()
    {
        ShowBanner();
    }

    void ShowBanner()
    {
        // Create a 320x50 banner at the top of the screen.
        BannerView bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
