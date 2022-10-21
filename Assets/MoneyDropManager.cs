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

    private List<GameObject> coins = new();

    private static MoneyDropManager instance;
    public static MoneyDropManager Instance => instance;

    public int maxBatch = 100;

    private void Awake()
    {
        instance = this;
    }

    public void DropCoins(int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform);
            coin.transform.Rotate(
                Random.Range(0f, 150f),
                Random.Range(0f, 150f), 
                Random.Range(0f, 150f));
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
        List<GameObject> toRemove = new List<GameObject>();
        foreach (GameObject o in coins.Take(num))
        {
            o.SetActive(false);
            toRemove.Add(o);
        }

        foreach (GameObject coin in toRemove)
        {
            coins.Remove(coin);
        }
    }
}
