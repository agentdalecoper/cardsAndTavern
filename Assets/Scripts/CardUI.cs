using System;
using System.Collections.Generic;
using Client;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public TextMesh damageText;
    public TextMesh hpText;
    public TextMesh nameText;
    
    public Image image;

    [NonSerialized] public Card card;

    private RectTransform rectTransform;


    public UnityEvent ActionCardClicked;
    
    public static event Action<CardUI, CardUI> ActionCardDraggedOn;

    [NonSerialized] public int cardPosition;


    private bool dragable;
    private Vector3 startAnchoredPosition;

    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private Camera mainCamera;
    private float CameraZDistance;
    private float counter = 0;

    public SpriteRenderer[] skillUis;
    public GameObject view;

    public SpriteRenderer cardFace;

    private Quaternion initialRotation;

    private void Awake()
    {
        ShowEmptyCardData();
    }

    private void Start()
    {
        // rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        CameraZDistance =
            mainCamera.WorldToScreenPoint(transform.position).z; //z axis of the game object for screen view
        initialRotation = transform.localRotation;
    }
    
    public void ShowEmptyCardData()
    {
        damageText.text = "";
        hpText.text = "";
        nameText.text = "";
        // card = null;
        view.SetActive(false);

        NullifyUis();
        // image.raycastTarget = defaultRaycastTarget;
        // image.sprite = null;
    }

    private void NullifyUis()
    {
        cardFace.gameObject.SetActive(false);
        cardFace.sprite = null;

        for (var i = 0; i < skillUis.Length; i++)
        {
            skillUis[i].sprite = null;
            skillUis[i].gameObject.SetActive(false);
        }
    }

    public void ShowCardData(Card cardToShow, int position,
        List<SkillObject> activeSkillObjects, int cost, bool cardInInventory)
    {
        NullifyUis();
 
        damageText.text = cardToShow.damage.ToString();
        hpText.text = cardToShow.hp.ToString();
        nameText.text = cardToShow.name;
        card = cardToShow;
        view.SetActive(true);
        
        if (cardToShow.side == Side.player)
        {
            // image.raycastTarget = true;
            dragable = false;
        }
        else
        {
            // image.raycastTarget = true;
            dragable = true;
        }
        
        for (var i = 0; i < activeSkillObjects.Count; i++)
        {
            skillUis[i].sprite = activeSkillObjects[i].sprite;
            skillUis[i].gameObject.SetActive(true);
        }
        

        // image.sprite = cardToShow.sprite;

        cardPosition = position;

        cardFace.sprite = cardToShow.cardObject.card.sprite;
        transform.localRotation = initialRotation;
        cardFace.gameObject.SetActive(true);
    }
    
    void OnMouseDown()
    {
        Debug.Log("Card clicked " + this);
        ActionCardClicked?.Invoke();

        if (!dragable)
        {
            return;
        }
        
        startAnchoredPosition = transform.position;
    }
    
    void OnMouseDrag()
    {
        if (!dragable)
        {
            return;
        }

        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraZDistance);
        Vector3 newWorldPosition = mainCamera.ScreenToWorldPoint(screenPosition); //Screen point converted to world point
        transform.position = new Vector3(newWorldPosition.x, transform.position.y, newWorldPosition.z);
        // transform.DOShakeRotation(0.1f, 0.1f, 1);
    }

    private void OnMouseOver()
    {
        if (card != null && card.side == Side.player )
        {
            transform.DOShakeRotation(0.1f, 0.5f, 10);
        }
    }

    private void OnMouseExit()
    {
        if (card != null && card.side == Side.player)
        {
            transform.DOLocalRotate(initialRotation.eulerAngles, 0.1f);
        }
    }


    public void OnMouseUp()
    {
        if (!dragable)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            CardUI otherCardUi = hit.collider.gameObject.GetComponent<CardUI>();
            if (otherCardUi != null && otherCardUi != this)
            {
                Debug.Log("Found raycast image!!! " + hit.collider.gameObject);
                ActionCardDraggedOn?.Invoke(this, otherCardUi);
                // transform.DOShakeRotation(0.1f, 2f, 10);
            }
        }
        //
        // foreach (RaycastResult result in raycastResults)
        // {
        //     // Debug.Log("checking raycast " + result.gameObject + " " + eventData.position);
        //
        //     CardUI otherCardUi = result.gameObject.GetComponent<CardUI>();
        //     if (otherCardUi != null && otherCardUi != this)
        //     {
        //         Debug.Log("Found raycast image!!! " + result.gameObject);
        //         ActionCardDraggedOn?.Invoke(this, otherCardUi);
        //     }
        // }

        transform.position = startAnchoredPosition;
        // transform.rotation = initialRotation;
    }

    public override string ToString()
    {
        return $"{card} position: {cardPosition}";
    }
}

// var pointerEvent = new PointerEventData(FindObjectOfType<EventSystem>())
// {
//     position = EcsStartup.Instance.sceneConfiguration.uiCamera.ScreenToWorldPoint(Input.mousePosition)
// };


// DeActivateDiceView();
// text.text = cardInfo.text;
// image.sprite = cardInfo.sprite;

/*
 *
public TextMeshProUGUI playerSayingText;
public Image enemyHp;

TaskCompletionSource<bool> isWaitingUiDelay = new TaskCompletionSource<bool>();

public static event Action<EcsEntity> ActionNewCardAppeared;
public static event Action<EcsEntity> ActionSwipedRight;
public static event Action<EcsEntity> ActionSwipedLeft;
    public Button buttonLeft;
public Button buttonRight;
 */


// private static CardUI instance;
// public static CardUI Instance => instance;

// private string leftOptionSaying;
// private string rightOptionSaying;

// private void Awake()
// {
//     instance = this;
//     
//     isWaitingUiDelay.SetResult(false);
//
//     buttonLeft.onClick.AddListener(delegate { ActionSwipedLeft?.Invoke(EcsEntity.Null); });
//     buttonRight.onClick.AddListener(delegate { ActionSwipedRight?.Invoke(EcsEntity.Null); });
//     
//     playerSayingText.gameObject.SetActive(false);
// }
//
//
// public async void ShowCardData(EcsEntity cardEntity, CardInfo cardInfo, PointsLeftRight pointsLeftRight)
// {
//     await isWaitingUiDelay.Task;
//     
//     DeActivateDiceView();
//     text.text = " Left points: " + pointsLeftRight.left + " Right points: " + pointsLeftRight.right;
// }
//
// public async void ShowCardData(EcsEntity cardEntity, CardInfo cardInfo, SkillsLeftRight skillsLeftRight)
// {
//     await isWaitingUiDelay.Task;
//
//     DeActivateDiceView();
//     text.text = " Left skill: " + skillsLeftRight.left + " Right skill: " + skillsLeftRight.right;
//     
// }
//
// public async void ShowCardData(EcsEntity cardEntity, CardInfo cardInfo, SkillsCheck skillsCheck, 
//     DialogOption? leftOption = null, DialogOption? rightOption = null)
// {
//     DeActivateDiceView();
//     text.text = cardInfo.text;
//     image.sprite = cardInfo.sprite;
//     diceView.text.text = 0.ToString();
//     enemyHp.fillAmount = 1f;
//
//
//     playerSayingText.gameObject.SetActive(false);
//     if (leftOption != null)
//     {
//         buttonLeft.GetComponentInChildren<TextMeshProUGUI>().text = leftOption.Value.text;
//         leftOptionSaying = leftOption.Value.text;
//     }
//
//     if (rightOption != null)
//     {
//         buttonRight.GetComponentInChildren<TextMeshProUGUI>().text = rightOption.Value.text;
//         rightOptionSaying = rightOption.Value.text;
//     }
// }

// public void StartPooledDamageTween(float armyHp, float armyMaxHp)
// {
//     enemyHp.fillAmount = armyHp * 1f / armyMaxHp;
// }

//
//
// public async void ShowDiceData(DiceRoll diceRoll, bool success,
//     SkillsCheck skillsCheck, SkillsComponent playerSkills)
// {
//     ActivateDiceView();
//     diceView.diceEnabled = false;
//
//     if (success)
//     {
//         diceView.GetComponent<Image>().color = Color.green;
//     }
//     else
//     {
//         diceView.GetComponent<Image>().color = Color.red;
//     }
//     
//     isWaitingUiDelay.TrySetResult(true);
//     diceView.text.text = diceRoll.roll.ToString();
//             
//     // buttonLeft.gameObject.SetActive(true);
//     // buttonRight.gameObject.SetActive(true);
//
//     string successOrLoose = success ? "Dice success" : "Dice lose";
//     // text.text = $"{successOrLoose} was checking {skillsCheck} vs. " +
//     //             $" playerSkills={playerSkills} + diceRoll={diceRoll.roll}";
//
//     await Task.Delay(TimeSpan.FromSeconds(20f));
//     isWaitingUiDelay.TrySetResult(false);
// }

// private void ActivateDiceView()
// {
//     diceView.gameObject.SetActive(true);
//     diceView.diceEnabled = true;
//     diceView.GetComponent<Image>().color = Color.white;
//
//     diceView.gameObject.SetActive(true);
//     // buttonLeft.gameObject.SetActive(false);
//     // buttonRight.gameObject.SetActive(false);
// }
//
// private void DeActivateDiceView()
// {
//     diceView.gameObject.SetActive(false);
//     // buttonLeft.gameObject.SetActive(true);
//     // buttonRight.gameObject.SetActive(true);
// }


// UISwipeableViewBasic.ActionSwipingRight += (s,f) =>
// {
//     playerSayingText.text = rightOptionSaying;
//     if (f < 0.2f)
//     {
//         playerSayingText.gameObject.SetActive(false);
//     }
//     else
//     {
//         playerSayingText.gameObject.SetActive(true);
//
//     }
// };
// UISwipeableViewBasic.ActionSwipingLeft += (s,f) =>
// {
//     playerSayingText.text = leftOptionSaying;
//     if (f < 0.2f)
//     {
//         playerSayingText.gameObject.SetActive(false);
//     }
//     else
//     {
//         playerSayingText.gameObject.SetActive(true);
//
//     }
// };