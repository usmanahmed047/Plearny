using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AdmobAds
{
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance;

        private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
        private DateTime appOpenExpireTime;
        private AppOpenAd appOpenAd;
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private RewardedInterstitialAd rewardedInterstitialAd;
        private float deltaTime;
        private bool isShowingAppOpenAd;
        public UnityEvent OnAdLoadedEvent;
        public UnityEvent OnAdFailedToLoadEvent;
        public UnityEvent OnAdOpeningEvent;
        public UnityEvent OnAdFailedToShowEvent;
        public UnityEvent OnUserEarnedRewardEvent;
        public UnityEvent OnAdClosedEvent;
        public bool showFpsMeter = true;

        [SerializeField] private string bannerAdId = "ca-app-pub-3940256099942544/6300978111";
        [SerializeField] private string interstitialAdId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string rewardedAdId = "ca-app-pub-3940256099942544/5224354917";



        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }
        /*public Text fpsMeter;
        public Text statusText;*/


        #region UNITY MONOBEHAVIOR METHODS

        public void Start()
        {
            //MobileAds.SetiOSAppPauseOnBackground(true);
            List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

            // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
            deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
            deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
#endif

            // Configure TagForChildDirectedTreatment and test device IDs.
            RequestConfiguration requestConfiguration =
                new RequestConfiguration.Builder()
                .SetSameAppKeyEnabled(true)
                //.SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
                /*.SetTestDeviceIds(deviceIds)*/.build();
            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(HandleInitCompleteAction);

            // Listen to application foreground / background events.
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            RequestAndLoadInterstitialAd();
            RequestAndLoadRewardedAd();
        }

        private void HandleInitCompleteAction(InitializationStatus initstatus)
        {
            Debug.Log("Initialization complete.");

            // Callbacks from GoogleMobileAds are not guaranteed to be called on
            // the main thread.
            // In this example we use MobileAdsEventExecutor to schedule these calls on
            // the next Update() loop.
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //statusText.text = "Initialization complete.";
                RequestBannerAd();
            });
        }


        public void Test()
        {
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
            string android_id = secure.CallStatic<string>("getString", contentResolver, "android_id");
        }

        #endregion

        #region HELPER METHODS

        private AdRequest CreateAdRequest()
        {
            return new AdRequest.Builder()
                .Build();
        }

        #endregion

        #region BANNER ADS

        public void RequestBannerAd()
        {
            PrintStatus("Requesting Banner ad.");

            // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = bannerAdId;
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create a 320x50 banner at top of the screen
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

            // Add Event Handlers
            bannerView.OnAdLoaded += (sender, args) =>
            {
                PrintStatus("Banner ad loaded.");
                OnAdLoadedEvent.Invoke();
            };
            bannerView.OnAdFailedToLoad += (sender, args) =>
            {
                PrintStatus("Banner ad failed to load with error: " + args.LoadAdError.GetMessage());
                OnAdFailedToLoadEvent.Invoke();
            };
            bannerView.OnAdOpening += (sender, args) =>
            {
                PrintStatus("Banner ad opening.");
                OnAdOpeningEvent.Invoke();
            };
            bannerView.OnAdClosed += (sender, args) =>
            {
                PrintStatus("Banner ad closed.");
                OnAdClosedEvent.Invoke();
            };
            bannerView.OnPaidEvent += (sender, args) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "Banner ad received a paid event.",
                                            args.AdValue.CurrencyCode,
                                            args.AdValue.Value);
                PrintStatus(msg);
            };

            // Load a banner ad
            bannerView.LoadAd(CreateAdRequest());
        }

        public void DestroyBannerAd()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
            }
        }

        public void ShowBannerAd(bool chk)
        {
            if (chk)
            {
                if (bannerView != null)
                {
                    PrintStatus("Banner show");
                    RequestBannerAd();
                    bannerView.Show();
                }
                else
                {
                    PrintStatus("Banner Ad is Null");
                }
            }
            else
            {
                PrintStatus("HIde Banner Ad");
                bannerView.Hide();
            }
        }
        #endregion

        #region INTERSTITIAL ADS

        public void RequestAndLoadInterstitialAd()
        {
            PrintStatus("Requesting Interstitial ad.");

#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = interstitialAdId;
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif

            // Clean up interstitial before using it
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }

            interstitialAd = new InterstitialAd(adUnitId);

            // Add Event Handlers
            interstitialAd.OnAdLoaded += (sender, args) =>
            {
                PrintStatus("Interstitial ad loaded.");
                OnAdLoadedEvent.Invoke();
            };
            interstitialAd.OnAdFailedToLoad += (sender, args) =>
            {
                PrintStatus("Interstitial ad failed to load with error: " + args.LoadAdError.GetMessage());
                OnAdFailedToLoadEvent.Invoke();
            };
            interstitialAd.OnAdOpening += (sender, args) =>
            {
                PrintStatus("Interstitial ad opening.");
                OnAdOpeningEvent.Invoke();
            };
            interstitialAd.OnAdClosed += (sender, args) =>
            {
                PrintStatus("Interstitial ad closed.");
                RequestAndLoadInterstitialAd();
                if (AdsCounter.Instance.timerAd)
                {
                    AdsCounter.Instance.InitializeAdsResetTime();
                    Time.timeScale = 0;
                }
                OnAdClosedEvent.Invoke();
            };
            interstitialAd.OnAdDidRecordImpression += (sender, args) =>
            {
                PrintStatus("Interstitial ad recorded an impression.");
            };
            interstitialAd.OnAdFailedToShow += (sender, args) =>
            {
                PrintStatus("Interstitial ad failed to show.");
            };
            interstitialAd.OnPaidEvent += (sender, args) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "Interstitial ad received a paid event.",
                                            args.AdValue.CurrencyCode,
                                            args.AdValue.Value);
                PrintStatus(msg);
            };

            // Load an interstitial ad
            interstitialAd.LoadAd(CreateAdRequest());
        }

        public void ShowInterstitialAd()
        {
            if (interstitialAd != null && interstitialAd.IsLoaded())
            {
                interstitialAd.Show();
            }
            else
            {
                PrintStatus("Interstitial ad is not ready yet.");
            }
        }

        public void DestroyInterstitialAd()
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }
        }

        public bool isInterstitialLoaded()
        {
            return interstitialAd != null && interstitialAd.IsLoaded();
        }


        #endregion

        #region REWARDED ADS

        public void RequestAndLoadRewardedAd()
        {
            PrintStatus("Requesting Rewarded ad.");
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = rewardedAdId;
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        string adUnitId = "unexpected_platform";
#endif

            // create new rewarded ad instance
            this.rewardedAd = new RewardedAd(adUnitId);

            // Add Event Handlers
            rewardedAd.OnAdLoaded += (sender, args) =>
            {
                PrintStatus("Reward ad loaded.");
                OnAdLoadedEvent.Invoke();
            };
            rewardedAd.OnAdFailedToLoad += (sender, args) =>
            {
                PrintStatus("Reward ad failed to load.");
                OnAdFailedToLoadEvent.Invoke();
            };
            rewardedAd.OnAdOpening += (sender, args) =>
            {
                PrintStatus("Reward ad opening.");
                OnAdOpeningEvent.Invoke();
            };
            rewardedAd.OnAdFailedToShow += (sender, args) =>
            {
                PrintStatus("Reward ad failed to show with error: " + args.AdError.GetMessage());
                OnAdFailedToShowEvent.Invoke();
            };
            rewardedAd.OnAdClosed += (sender, args) =>
            {
                PrintStatus("Reward ad closed.");
                RequestAndLoadRewardedAd();
                /*if (AdsCounter.Instance.timerAd)
                {
                    Time.timeScale = 0;
                    GrantRewardToPlayer();
                    AdsCounter.Instance.InitializeAdsResetTime();
                }*/
                OnAdClosedEvent.Invoke();
            };
            rewardedAd.OnUserEarnedReward += (sender, args) =>
            {
                PrintStatus("User earned Reward ad reward: " + args.Amount);
                RequestAndLoadRewardedAd();
                if (!AdsCounter.Instance.isIdle)
                    GrantRewardToPlayer();
                if (MainController.instance.continueFrame.rewardClick)
                {
                    MainController.instance.continueFrame.rewardClick = false;
                    MainController.instance.continueFrame.HandleRewardBasedVideoRewarded();
                }
                OnUserEarnedRewardEvent.Invoke();
            };
            rewardedAd.OnAdDidRecordImpression += (sender, args) =>
            {
                PrintStatus("Reward ad recorded an impression.");
            };
            rewardedAd.OnPaidEvent += (sender, args) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "Rewarded ad received a paid event.",
                                            args.AdValue.CurrencyCode,
                                            args.AdValue.Value);
                PrintStatus(msg);
            };

            // Create empty ad request
            rewardedAd.LoadAd(CreateAdRequest());
        }

        public void ShowRewardedAd()
        {
            if (rewardedAd != null && rewardedAd.IsLoaded())
            {
                PrintStatus("Rewarded ad Shown");
                rewardedAd.Show();
            }
            else
            {
                PrintStatus("Rewarded ad is not ready yet.");
                if (!rewardedAd.IsLoaded())
                {
                    RequestAndLoadRewardedAd();
                }
            }
        }

        public void RequestAndLoadRewardedInterstitialAd()
        {
            PrintStatus("Requesting Rewarded Interstitial ad.");

            // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
            string adUnitId = "unexpected_platform";
#endif

            // Create an interstitial.
            RewardedInterstitialAd.LoadAd(adUnitId, CreateAdRequest(), (rewardedInterstitialAd, error) =>
            {
                if (error != null)
                {
                    PrintStatus("Rewarded Interstitial ad load failed with error: " + error);
                    return;
                }

                this.rewardedInterstitialAd = rewardedInterstitialAd;
                PrintStatus("Rewarded Interstitial ad loaded.");

                // Register for ad events.
                this.rewardedInterstitialAd.OnAdDidPresentFullScreenContent += (sender, args) =>
                {
                    PrintStatus("Rewarded Interstitial ad presented.");
                };
                this.rewardedInterstitialAd.OnAdDidDismissFullScreenContent += (sender, args) =>
                {
                    PrintStatus("Rewarded Interstitial ad dismissed.");
                    this.rewardedInterstitialAd = null;
                };
                this.rewardedInterstitialAd.OnAdFailedToPresentFullScreenContent += (sender, args) =>
                {
                    PrintStatus("Rewarded Interstitial ad failed to present with error: " +
                                                                            args.AdError.GetMessage());
                    this.rewardedInterstitialAd = null;
                };
                this.rewardedInterstitialAd.OnPaidEvent += (sender, args) =>
                {
                    string msg = string.Format("{0} (currency: {1}, value: {2}",
                                                "Rewarded Interstitial ad received a paid event.",
                                                args.AdValue.CurrencyCode,
                                                args.AdValue.Value);
                    PrintStatus(msg);
                };
                this.rewardedInterstitialAd.OnAdDidRecordImpression += (sender, args) =>
                {
                    PrintStatus("Rewarded Interstitial ad recorded an impression.");
                };
            });
        }

        public void ShowRewardedInterstitialAd()
        {
            if (rewardedInterstitialAd != null)
            {
                rewardedInterstitialAd.Show((reward) =>
                {
                    PrintStatus("Rewarded Interstitial ad Rewarded : " + reward.Amount);
                });
            }
            else
            {
                PrintStatus("Rewarded Interstitial ad is not ready yet.");
            }
        }

        public bool isRewardVideoLoaded()
        {
            return rewardedAd != null && rewardedAd.IsLoaded();
        }

        #endregion

        #region APPOPEN ADS

        public bool IsAppOpenAdAvailable
        {
            get
            {
                return (!isShowingAppOpenAd
                        && appOpenAd != null
                        && DateTime.Now < appOpenExpireTime);
            }
        }

        public void OnAppStateChanged(AppState state)
        {
            // Display the app open ad when the app is foregrounded.
            UnityEngine.Debug.Log("App State is " + state);

            // OnAppStateChanged is not guaranteed to execute on the Unity UI thread.
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (state == AppState.Foreground)
                {
                    ShowAppOpenAd();
                }
            });
        }

        public void RequestAndLoadAppOpenAd()
        {
            PrintStatus("Requesting App Open ad.");
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/3419835294";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/5662855259";
#else
        string adUnitId = "unexpected_platform";
#endif
            // create new app open ad instance
            AppOpenAd.LoadAd(adUnitId,
                             ScreenOrientation.Portrait,
                             CreateAdRequest(),
                             OnAppOpenAdLoad);
        }

        private void OnAppOpenAdLoad(AppOpenAd ad, AdFailedToLoadEventArgs error)
        {
            if (error != null)
            {
                PrintStatus("App Open ad failed to load with error: " + error);
                return;
            }

            PrintStatus("App Open ad loaded. Please background the app and return.");
            this.appOpenAd = ad;
            this.appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;
        }

        public void ShowAppOpenAd()
        {
            if (!IsAppOpenAdAvailable)
            {
                return;
            }

            // Register for ad events.
            this.appOpenAd.OnAdDidDismissFullScreenContent += (sender, args) =>
            {
                PrintStatus("App Open ad dismissed.");
                isShowingAppOpenAd = false;
                if (this.appOpenAd != null)
                {
                    this.appOpenAd.Destroy();
                    this.appOpenAd = null;
                }
            };
            this.appOpenAd.OnAdFailedToPresentFullScreenContent += (sender, args) =>
            {
                PrintStatus("App Open ad failed to present with error: " + args.AdError.GetMessage());

                isShowingAppOpenAd = false;
                if (this.appOpenAd != null)
                {
                    this.appOpenAd.Destroy();
                    this.appOpenAd = null;
                }
            };
            this.appOpenAd.OnAdDidPresentFullScreenContent += (sender, args) =>
            {
                PrintStatus("App Open ad opened.");
            };
            this.appOpenAd.OnAdDidRecordImpression += (sender, args) =>
            {
                PrintStatus("App Open ad recorded an impression.");
            };
            this.appOpenAd.OnPaidEvent += (sender, args) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "App Open ad received a paid event.",
                                            args.AdValue.CurrencyCode,
                                            args.AdValue.Value);
                PrintStatus(msg);
            };

            isShowingAppOpenAd = true;
            appOpenAd.Show();
        }

        #endregion


        #region AD INSPECTOR

        public void OpenAdInspector()
        {
            PrintStatus("Open ad Inspector.");

            MobileAds.OpenAdInspector((error) =>
            {
                if (error != null)
                {
                    PrintStatus("ad Inspector failed to open with error: " + error);
                }
                else
                {
                    PrintStatus("Ad Inspector opened successfully.");
                }
            });
        }

        #endregion

        #region Utility

        ///<summary>
        /// Log the message and update the status text on the main thread.
        ///<summary>
        private void PrintStatus(string message)
        {
            Debug.Log(message);
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //statusText.text = message;
            });
        }

        #endregion

        #region CustomCallbacks

        private void GrantRewardToPlayer()
        {
            // Reward Granted
            PrintStatus("Reward Granted");
            PlayerPrefs.SetInt("Cash", PlayerPrefs.GetInt("Cash", 0) + 5);
            if (AdsCounter.Instance.timerAd)
            {
                Time.timeScale = 0;
                AdsCounter.Instance.InitializeAdsResetTime();
            }

        }


        #endregion

    }
}