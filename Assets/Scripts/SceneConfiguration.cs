using System;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Client
{
    [Serializable]
    public class  Level
    {
        public Optional<DialogObject> dialogObject;
        public Optional<ChoseCardsObject> chooseCardsObject;
        public Optional<EnemyCardsObject> enemyCardsObject;

        public UnityEvent levelStartEvents;
        public UnityEvent levelEndEvents;
    }
    
    [Serializable]
    public class SceneConfiguration
    {
        public Level[] levels;

        public CardObject[] cardsPlayer;
        
        public int cardsOnBoardCount = 5;
        public int maxCardsInAllSideLines = 15;
        
        public Transform playerCardsHolder;
        public Transform enemyCardsHolder;

        public TextMesh hpBalanceManagerText;

        public GameObject mainSceneHolder;

        public GameObject mainGameObject;

        public Canvas endGameCanvas;
       
        public TextMeshProUGUI dialogText;
        public TextMeshProUGUI endText;
        public Image endGameImage;

        public AudioClip cardDamageAudio;
        public AudioSource tableAudioSource;

        public Transform cardsChooseHolder;
        public CardUI choosenCardCameraOverlay;
        public bool clickedNextLevel;

        public CinemachineBrain cinemachineBrain;
        public CinemachineVirtualCamera tableVirtualCamera;
        public CinemachineVirtualCamera forwardVirtualCamera;
        public CinemachineVirtualCamera leftwardVirtualCamera;
        public CinemachineVirtualCamera rightwardVirtualCamera;
        public float cinemaBlendingTime;
        public float cinemaBlendingLongTime;

        public Monster monster;
        public AudioSource audioSource;
        public GridScroller planeGrid;
        public AudioSource mystAudioSource;

        public SkillsObjectsListed skillsObjectsDict;

        public Shop shop;

        public SceneAudioConfiguration sceneAudioConfiguration;

        public SceneEffects sceneEffects;
    }

    [Serializable]
    public class SceneEffects
    {
        public GameObject playerStartGo;
        public GameObject enemyStartGo;
        public GameObject inventoryStartGo;
        
        public GameObject poisonCloud;
        public GameObject arrowShot;
        public GameObject buffOrHeal;
    }

    [Serializable]
    public class SceneAudioConfiguration
    {
        public AudioClip tavernAmbient;
        public AudioClip cardTake;
        public AudioClip cardDrop;
        public AudioClip cardMove;
        public AudioClip cardAttack;

        public AudioClip moneySound;
        public AudioClip medievalSound;
    }


    [Serializable]
    public class Shop
    {
        public int buyCardCost;
        public int buyIncomeCost;

        public int currentIncome;
        public int currentMoney;
        public int sacrificeCost;

        public TextMesh currentMoneyUI;
        public ShopCardUI buyCardUI;
        public ShopCardUI buyIncomeUI;

        public ChoseCardsObject shopChoseCardsObject;

        public GameObject inventoryCardsHolder;
        public GameObject shopGameObject;
    }

    [Serializable]
    public class SkillsObjectsListed
    {
        public SkillObject splitAttack;
        public SkillObject poisonOther;
        public SkillObject deadlyPoison;
        public SkillObject buff;
        public SkillObject transformation;
        public SkillObject arrowShot;
        public SkillObject steroids;
        public SkillObject summon;
        public SkillObject shield;
        public SkillObject healOther;
        public SkillObject horseRide;
        public SkillObject gyroAttack;
    }

    [Serializable]
    public class ChoseCardsObject
    {
        public CardObject[] cardsToChoseFrom;
    }

    [Serializable]
    public class EnemyCardsObject
    {
        public CardObject[] cardsEnemy;
    }
}