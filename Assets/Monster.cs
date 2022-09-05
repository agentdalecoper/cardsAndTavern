using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Monster : MonoBehaviour
{
    public Vector3 travelTo;

    public Material faceMaterial;
    public Material eyesMaterial;
    private static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");

    public UnityEvent monsterReachedPlayerEvent;

    public float monsterReachedPlayerScale = 48f;
    public float speed = 2f;

    private void Start()
    {
        faceMaterial.color = Color.white;
        eyesMaterial.SetColor(BackgroundColor, Color.red);
    }

    void Update()
    {
        transform.localScale += new Vector3(speed * Time.deltaTime, speed * Time.deltaTime, speed * Time.deltaTime);

        if (transform.localScale.x >= monsterReachedPlayerScale)
        {
            monsterReachedPlayerEvent?.Invoke();
        }
    }

    private void OnDestroy()
    {
        faceMaterial.color = Color.black;
        eyesMaterial.SetColor(BackgroundColor, Color.black);
    }
}