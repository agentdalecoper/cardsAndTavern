using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WebGlPlayer : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public Button nextButton;
    public TextMeshProUGUI sayingText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI playGameText;
    public GameObject catSign;

    public string animToPlay;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(animToPlay))
        {
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "TabernOut.mp4");

            nextButton.onClick.AddListener(() =>
            {
                PlayClicked();
                WaitingForDisabling();
            });
        }
        else
        {
            // videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, animToPlay + ".mp4");
            // videoPlayer.gameObject.SetActive(true);
            // videoPlayer.Play();
        }
    }

    private void PlayClicked()
    {
        titleText.gameObject.SetActive(false);
        catSign.SetActive(false);
        playGameText.gameObject.SetActive(false);
        sayingText.gameObject.SetActive(false);
        videoPlayer.gameObject.SetActive(false);
        // videoPlayer.Play();

        Debug.Log("Started playing " + videoPlayer);
    }

    private async void WaitingForDisabling()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f), ignoreTimeScale: false);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}