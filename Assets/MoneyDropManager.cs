using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoneyDropManager : MonoBehaviour
{
    public GameObject coinPrefab;

    private List<GameObject> coins = new List<GameObject>();

    private static MoneyDropManager instance;
    public static MoneyDropManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void DropCoins(int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform);
            coin.transform.Rotate(Random.Range(0f, 30f),
                Random.Range(0f, 30f), 
                Random.Range(0f, 30f));
            coin.transform.Translate(coin.transform.localPosition 
                                     + new Vector3(
                                         Random.Range(0f, 0.5f), 
                                         Random.Range(0f, 0.5f),
                                         Random.Range(0f, 0.5f)));
            coins.Add(coin);
        }
    }

    public void RemoveCoins(int num)
    {
        foreach (GameObject o in coins.Take(num))
        {
            o.SetActive(false);
            coins.Remove(o);
        }
    }
}
