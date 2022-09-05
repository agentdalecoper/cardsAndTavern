using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnClickEvent : MonoBehaviour
{
    public UnityEvent ActionClicked;

    private void OnMouseDown()
    {
        ActionClicked?.Invoke();
    }
}
