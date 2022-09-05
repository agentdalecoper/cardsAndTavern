using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsChooseUI : MonoBehaviour
{
     [NonSerialized]
     public static CardsChooseUI Instance;

     private void Awake()
     {
          Instance = this;
     }
     
     
     
     
}
