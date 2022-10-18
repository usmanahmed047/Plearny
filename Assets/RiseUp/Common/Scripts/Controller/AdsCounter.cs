using AdmobAds;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AdsCounter : MonoBehaviour
{
    [SerializeField] private int adsResetTime = 120;
    [SerializeField] private int timeOut = 120;
    [HideInInspector]
    public bool isIdle;
    private bool isResetRunning = false;
    private bool istimeOutRunning = false;

    public GameObject timeFrame;
    public GameObject pauseFrame;

    public static AdsCounter Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Start()
    {
        InitializeAdsResetTime();
        InitializeTimeOutTime();
    }

    public void InitializeAdsResetTime()
    {
        adsResetTime = 120;
        adRunning = false;
        timerAd = false;
        UnLoadTimer();
        //ShowPauseFrame(false);
    }

    private void InitializeTimeOutTime()
    {
        timeOut = 120;
        isIdle = false;
    }
    [HideInInspector]
    public bool adRunning;
    [HideInInspector]
    public bool timerAd;
    public async void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            InitializeTimeOutTime();
        }
        else
        {
            if (!istimeOutRunning)
            {
                if (timeOut > 0)
                {
                    istimeOutRunning = true;
                    await Task.Delay(1000);
                    timeOut -= 1;
                    isIdle = false;
                    istimeOutRunning = false;
                }
                else
                {
                    isIdle = true;
                    istimeOutRunning = false;
                }
            }
        }

        if (!adRunning)
        {
            if (!isResetRunning)
            {
                if (adsResetTime > 0)
                {
                    isResetRunning = true;
                    await Task.Delay(1000);
                    adsResetTime -= 1;
                    isResetRunning = false;
                }
                else
                {
                    //ShowAds();
                    StartCoroutine(LoadTimer());
                    adRunning = true;
                }
            }
        }


    }
    public List<GameObject> timeSprites = new List<GameObject>();

    IEnumerator LoadTimer()
    {
        timeFrame.SetActive(true);
        for (int i = 0; i < timeSprites.Count; i++)
        {
            timeSprites[i].SetActive(true);
            yield return new WaitForSecondsRealtime(1);
            timeSprites[i].SetActive(false);
        }
        ShowPauseFrame(true);
        ShowAds();

    }

    public void ShowPauseFrame(bool active)
    {
        pauseFrame.SetActive(active);
        Time.timeScale = active ? 0 : 1;
    }

    private void UnLoadTimer()
    {
        timeFrame.SetActive(false);
        for (int i = 0; i < timeSprites.Count; i++)
        {
            timeSprites[i].SetActive(false);
        }
    }

    private void ShowAds()
    {
        if (AdsManager.Instance.isRewardVideoLoaded() && AdsManager.Instance.isInterstitialLoaded())
        {
            if (Random.Range(0, 2) == 0)
            {
                timerAd = true;
                AdsManager.Instance.ShowRewardedAd();
                adRunning = true;
            }
            else
            {
                timerAd = true;
                AdsManager.Instance.ShowInterstitialAd();
                adRunning = true;
            }
        }
        else if (AdsManager.Instance.isRewardVideoLoaded())
        {
            timerAd = true;
            AdsManager.Instance.ShowRewardedAd();
            adRunning = true;
        }
        else if (AdsManager.Instance.isInterstitialLoaded())
        {
            timerAd = true;
            AdsManager.Instance.ShowInterstitialAd();
            adRunning = true;
        }
        Time.timeScale = 0;
    }

}
