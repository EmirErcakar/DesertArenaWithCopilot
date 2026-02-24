using UnityEngine;
using System;

/// <summary>
/// Stub ad service — immediately calls the success/complete callback.
/// Replace this with your real ad SDK implementation for production.
///
/// Inspector Setup:
///   - No required fields.
///   - Attach to the GameManager or a dedicated Ads GameObject.
///
/// To use real ads: create a class that implements IAdService and
/// swap this reference in any script that calls AdServiceStub.Instance.
/// </summary>
public class AdServiceStub : MonoBehaviour, IAdService
{
    public static AdServiceStub Instance { get; private set; }

    public bool IsRewardedAdReady => true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowRewardedAd(Action onSuccess)
    {
        Debug.Log("[AdStub] Rewarded ad shown (stub) — reward granted immediately.");
        onSuccess?.Invoke();
    }

    public void ShowInterstitialAd(Action onComplete)
    {
        Debug.Log("[AdStub] Interstitial ad shown (stub) — complete called immediately.");
        onComplete?.Invoke();
    }
}
