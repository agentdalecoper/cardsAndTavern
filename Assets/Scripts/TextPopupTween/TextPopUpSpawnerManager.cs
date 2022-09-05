using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextPopUpSpawnerManager : MonoBehaviour
{
    public List<TextPopUpTween> damageTweens;
    public int poolCont = 100;
    public TextPopUpTween textPopUpTweenPrefab;

    private static TextPopUpSpawnerManager instance;
    public static TextPopUpSpawnerManager Instance => instance;

    public void Awake()
    {
        instance = this;

        for (int i = 0; i < poolCont; i++)
        {
            TextPopUpTween textPopUpTween = GameObject.Instantiate(textPopUpTweenPrefab);
            textPopUpTween.gameObject.transform.SetParent(transform);
            textPopUpTween.gameObject.SetActive(false);
            damageTweens.Add(textPopUpTween);
        }
    }

    public void StartTextPopUpTween(string text, Color color, Transform parent)
    {
        TextPopUpTween textPopUpTween = damageTweens.FirstOrDefault(d => !d.gameObject.activeSelf);
        // textPopUpTween.gameObject.transform.position = whereToSpawn;
        textPopUpTween.gameObject.transform.SetParent(parent, false);
        textPopUpTween.gameObject.transform.localPosition = new Vector3();
        textPopUpTween.popUpText = text;
        textPopUpTween.text.color = color;
        textPopUpTween.gameObject.SetActive(true);
        textPopUpTween.Activate();
    }
    
    public void ShowTextPopUpTween(string text, Color color, Transform parent)
    {
        TextPopUpTween textPopUpTween = damageTweens.FirstOrDefault(d => !d.gameObject.activeSelf);
        // textPopUpTween.gameObject.transform.position = whereToSpawn;
        textPopUpTween.gameObject.transform.SetParent(transform, false);
        textPopUpTween.gameObject.transform.localPosition = new Vector3();
        textPopUpTween.popUpText = text;
        textPopUpTween.text.color = color;
        textPopUpTween.gameObject.SetActive(true);
    }
}