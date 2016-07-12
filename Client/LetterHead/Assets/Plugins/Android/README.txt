=======================================================
Unity Ads Adapter for Google Mobile Ads SDK for Android
=======================================================

This is an adapter to be used in conjunction with the Google Mobile Ads
SDK in Google Play services.

Requirements:
- Android SDK 3.2 or later
- Google Play services 8.3 or later
- Unity Ads SDK

Instructions:
- Add the adapter jar into your Android project.
- Import the Unity Ads library project into your Android project (Detailed
  instructions on how to import Unity Ads library are available at:
  http://unityads.unity3d.com/help/monetization/integration-guide-android).
- Enable the Ad network in the Ad Network Mediation UI.
- Unity Ads SDK does not provide a reward value when rewarded video is
  completed, so the adapter defaults to a reward of type "" with value 1. Please
  override the reward value in the AdMob console.

Additional Code Required:
- *Required only if your app targets Android API 13 or below* Call
  UnityAds.changeActivity(Activity) method in the Activity's onResume() method.
- This needs to be added in all the Activities that intend to show ads.

Example:

@Override
public void onResume() {
  super.onResume();
  UnityAds.changeActivity(this);
}

The latest documentation and code samples for the Google Mobile Ads SDK are
available at: https://developers.google.com/admob/android/quick-start
