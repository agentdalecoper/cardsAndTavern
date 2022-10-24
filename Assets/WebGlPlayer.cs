using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.gameObject.SetActive(false);

        nextButton.onClick.AddListener(() =>
        {
            videoPlayer.gameObject.SetActive(true);

            nextButton.onClick.RemoveAllListeners();
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Won.mp4");
            videoPlayer.Play();
            sayingText.text = "It was a 1254 year after Christ birth in the nameless country in the nameless place.";

            titleText.gameObject.SetActive(false);
            catSign.SetActive(false);

            playGameText.text = "Next";

            sayingText.gameObject.SetActive(true);

            nextButton.onClick.AddListener(() =>
            {
                nextButton.onClick.RemoveAllListeners();
                videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "TabernOut.mp4");
                sayingText.text = "I was walking to my tavern to play card game we liked. It was called - Apocalypse.";

                nextButton.onClick.AddListener(() =>
                {
                    nextButton.onClick.RemoveAllListeners();
                    gameObject.transform.parent.gameObject.SetActive(false);
                });
            });
        });
    }
}