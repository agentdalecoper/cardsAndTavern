using System;
using Client;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    [SerializeField] private bool playerEntered;

    public static event Action onTableKeyPressed;

    public AudioSource audioSource;

    public AudioClip switchUiAudioClip;

    private void Awake()
    {
        CameraController.onUiSwitched += () =>
        {
            audioSource.PlayOneShot(switchUiAudioClip);
        };
    }

    void Update()
    {
        if (playerEntered && Input.GetKeyDown("f"))
        {
            Debug.Log("Key pressed f");
            onTableKeyPressed?.Invoke();

            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On trigger enter table");
        playerEntered = true;
    }


    private void OnTriggerExit(Collider other)
    {
        Debug.Log("On trigger exit table");
        playerEntered = false;
    }
}