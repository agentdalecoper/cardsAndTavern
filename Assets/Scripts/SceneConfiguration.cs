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
        public Optional<Tutorial> tutorial;
        public Optional<DialogObject> dialogObject;
        public Optional<ChoseCardsObject> chooseCardsObject;
        public Optional<EnemyCardsObject> enemyCardsObject;

        public UnityEvent levelStartEvents;
        public UnityEvent levelEndEvents;

        public int levelIncome;

        public bool isReward;
    }

    [Serializable]
    public class Tutorial
    {
        public CardObject tutorialCard;
        
        public string dragFromShopSayingText;
        public string tryRollingText;
        public string yourCardsWillRespawnText;
        public string gradeYourCard;
        public string youHaveGradedCard;
        public string youHaveInventorySayingText;
        public string youCanSellCard;
        public string youHaveHpText;
        public string nowClickNextLevelText;
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

        public Canvas intorGameCanvas;

        public Canvas gameIsWonCanvas;
        public Canvas gameIsLostCanvas;
       
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

        public TutorialConfiguration tutorialConfiguration;
    }

    [Serializable]
    public class TutorialConfiguration
    {
        public GameObject auraEffect;
        public CardInfoUI cardInfoUI;
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
        public int buyCardInitialCost;
        public int buyCardCostStep;
        public int numberTimesRolled;
        public int buyIncomeCost;

        public int currentIncome;
        public int currentMoney;
        public int sacrificeCost;

        public TextMesh currentMoneyUI;
        public ShopCardUI buyCardUI;
        public ShopCardUI buyIncomeUI;

        public ChoseCardsObject shopChoseCardsObject;

        public GameObject nextLevelButtonObject;
        public GameObject rollACardHolder;
        public GameObject sellCardHolderObject;
        public GameObject inventoryCardsHolder;
        public GameObject shopGameObject;
        
        public CardObject[] inventoryRewardObjects;
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
        public SkillObject income;
    }

    [Serializable]
    public class ChoseCardsObject
    {
        public CardObject[] cardsToChoseFrom;
    }

    [Serializable]
    public class EnemyCardsObject
    {
        public NumAndCard[] numAndCards;
    }

    [Serializable]
    public class NumAndCard
    {
        public CardObject cardObject;
        public int num;
    }
}