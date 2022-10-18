﻿//using GoogleMobileAds.Api;
using AdmobAds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinueFrame : MonoBehaviour {

    public Image progressBar;
    public Text timer;
    private int timeValue ;
    private bool timeRunning;
    private double startTime;
    public GameObject content;
    private bool rewardSuccess = false;
    public bool rewardClick = false;

    public void HandleRewardBasedVideoClosed()
    {
        if(!rewardSuccess)
            OnNoClick();
    }

    public void HandleRewardBasedVideoRewarded()
    {
        rewardSuccess = true;
        RewardSuccess();
    }

    private void RewardSuccess()
    {
        MainController.instance.ContinueLastGame();
    }

    public void OnShowRewardedClick()
    {
        timeRunning = false;
        content.SetActive(false);
        rewardSuccess = false;
        rewardClick = true;
#if UNITY_EDITOR
        HandleRewardBasedVideoRewarded();
#else
        AdsManager.Instance.ShowRewardedAd();
#endif
        Sound.instance.PlayButton();
    }

    public void OnNoClick()
    {
        timeRunning = false;
        content.SetActive(false);
        MainController.instance.GameOver(false);
    }

    public void ShowContinueFrame()
    {
        content.SetActive(true);
        timeValue = 10;
        timeRunning = true;
        startTime = CUtils.GetCurrentTime();
        StartCoroutine(IERunCountDown());
    }

    void Update()
    {
        if(timeRunning)
        {
            float passTime = (float)(CUtils.GetCurrentTime() - startTime);
            progressBar.fillAmount = (1f - Mathf.Clamp01(passTime / 10));
        }
    }

    private IEnumerator IERunCountDown()
    {
        while (timeRunning)
        {
            UpdateText();
            yield return new WaitForSeconds(1);
            if (timeValue <= 0 || !timeRunning)
            {
                if(timeRunning)
                    OnNoClick();
                break;
            }
            else timeValue--;
        }
    }

    private void UpdateText()
    {
        timer.text = timeValue.ToString();
    }
}
