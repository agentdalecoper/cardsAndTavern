using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextPopUpTween : MonoBehaviour
{
    public string popUpText;
    public TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Activate()
    {
        text.text = popUpText;
        // float randPosX = Random.Range(-10, 10);
        // float randPosY = Random.Range(-10, 10);
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
            gameObject.transform.position.y, 0f);

        gameObject.transform.DOPunchPosition(Vector3.up, 1f);
        float randomPlusY = Random.Range(0, 5);
        gameObject.transform.DOLocalMoveY(gameObject.transform.localPosition.y + 400f + randomPlusY, 1f)
            .OnComplete(() => gameObject.SetActive(false));
    }
}