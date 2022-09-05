using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScroller : MonoBehaviour
{
    private static readonly int Offset = Shader.PropertyToID("_Offset");

    public Material material;

    public float offsetFlow;

    private Color initCol1;
    private Color initCol2;

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("called");
        material.SetFloat(Offset, material.GetFloat(Offset) + offsetFlow * 0.01f);   
    }

    private void Start()
    {
        initCol1 = material.GetColor("_MainColor");
        initCol2 = material.GetColor("_SecondaryColor");
    }

    public void MakeScary()
    {
        offsetFlow *= 10f;
        material.SetColor("_MainColor", Color.red);
        material.SetColor("_SecondaryColor", Color.red);
    }

    private void OnDestroy()
    {
        material.SetColor("_MainColor", initCol1);
        material.SetColor("_SecondaryColor", initCol2);
    }
}
