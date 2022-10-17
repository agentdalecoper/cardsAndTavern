using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoUI : MonoBehaviour
{
    public CardUI cardUI;
    public SkillDescriptionUI[] skillDescriptionUis;
    public TextMesh cardDesctiption;

    public void NullifyUIs()
    {
        cardUI.card = null;
        cardUI.ShowEmptyCardData();
        for (int i = 0; i < skillDescriptionUis.Length; i++)
        {
            SkillDescriptionUI skillDescriptionUI = skillDescriptionUis[i];
            skillDescriptionUI.gameObject.SetActive(false);
        }

    }
}