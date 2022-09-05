using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopCardUI : MonoBehaviour
{
    public TextMesh costText;
    public TextMesh cardNameText;
    
    public UnityEvent actionCardClicked;

    void OnMouseDown()
    {
        Debug.Log("Card shop clicked " + this);
        actionCardClicked?.Invoke();
    }

}
