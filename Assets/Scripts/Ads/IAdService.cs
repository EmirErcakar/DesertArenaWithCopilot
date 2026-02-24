using System;

/// <summary>
/// Interface for the ad service.
/// Implement this interface when integrating a real ad SDK (Google AdMob, Unity Ads, etc.).
/// </summary>
public interface IAdService
{
    /// <summary>True when a rewarded ad is loaded and ready to show.</summary>
    bool IsRewardedAdReady { get; }

    /// <summary>
    /// Show a rewarded ad. Call onSuccess if the user watches to completion.
    /// Called for: revive, reroll, chest x2.
    /// </summary>
    void ShowRewardedAd(Action onSuccess);

    /// <summary>
    /// Show an interstitial ad (no reward).
    /// Called onComplete regardless of outcome.
    /// </summary>
    void ShowInterstitialAd(Action onComplete);
}
