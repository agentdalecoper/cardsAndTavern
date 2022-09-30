using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client;
using Leopotam.Ecs;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

sealed class MainGameManager : MonoBehaviour, IEcsSystem
{
    EcsWorld _world;
    EcsSystems _systems;

    [SerializeField] private SceneConfiguration sceneConfiguration;

    [SerializeField] private GameContext gameContext;

    private static MainGameManager instance;
    public static MainGameManager Instance => instance;

    private CardsInvasionController cardsInvasionController;
    private CardsChoseController cardsChoseController;
    private DialogController dialogController;
    private CameraController cameraController;
    private ShopController shopController;
    private InitializeCardSystem initializeCardSystem;

    private void Awake()
    {
        instance = this;
        sceneConfiguration.mainSceneHolder.SetActive(true);
    }

    async void Start()
    {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

#if UNITY_EDITOR
        Leopotam.Ecs.UnityIntegration.EcsWorldObserver.Create(_world);
        Leopotam.Ecs.UnityIntegration.EcsSystemsObserver.Create(_systems);
#endif

        CardsInvasionController invasionController = new CardsInvasionController();
        CardsChoseController choseController = new CardsChoseController();
        DialogController dialogCntrler = new DialogController();
        CardsSystem cardsSystem = new CardsSystem();
        InitializeCardSystem initializeCardSystm = new InitializeCardSystem();
        CameraController cameraControllr = new CameraController();
        ShopController shopSystm = new ShopController();

        _systems
            .Add(invasionController)
            .Add(choseController)
            .Add(dialogCntrler)
            .Add(cardsSystem)
            .Add(initializeCardSystm)
            .Add(cameraControllr)
            .Add(DialogTextManager.Instance)
            .Add(this)
            .Add(shopSystm)
            .Inject(invasionController)
            .Inject(choseController)
            .Inject(dialogCntrler)
            .Inject(cardsSystem)
            .Inject(initializeCardSystm)
            .Inject(cameraControllr)
            .Inject(sceneConfiguration)
            .Inject(gameContext)
            .Inject(shopSystm)
            .Inject(DialogTextManager.Instance)
            .Init();

        StartFunction();

        // todo it can be better implemented via interfaces 
        CardUI.ActionCardDraggedOn += (draggedCard, underCard) =>
        {
            if (underCard.gameObject.name == "SellCard" && draggedCard.card.side == Side.player)
            {
                Debug.Log("Sell a card ActionCardDraggedOn");
                shopController.SellACardClicked(draggedCard);
                return;
            }

            // if card is from the shop and it is empty slot - buy card
            if (CardsSystem.isDeadOrEmpty(underCard.card) && draggedCard.card.side == Side.shop)
            {
                Debug.Log("Buy a card ActionCardDraggedOn");
                shopController.BuyCardClicked(draggedCard, underCard);
                return;
            }

            if (!CardsSystem.isDeadOrEmpty(underCard.card) && underCard.card.side == Side.enemy)
            {
                Debug.LogWarning("Tried to drag onto the enemy");
                return;
            }

            if (CardsSystem.isDeadOrEmpty(underCard.card))
            {
                Debug.Log("Move Card And Remove ActionCardDraggedOn");
                initializeCardSystm.MoveCardAndRemove(draggedCard, underCard);
                return;
            }

            if (!CardsSystem.isDeadOrEmpty(underCard.card) && draggedCard.card.itemOnly.IsSet
                                                           && draggedCard.card.itemOnly.Value.itemAddsSkill.IsSet)
            {
                shopController.AddSkillToACard(draggedCard, underCard);
                Debug.Log("Add a skill to a card ActionCardDraggedOn");
                return;
            }
            
            if (!CardsSystem.isDeadOrEmpty(underCard.card))
            {
                Debug.Log("Swap cards ActionCardDraggedOn");
                initializeCardSystm.SwapCards(draggedCard, underCard);
                return;
            }

        };

        test();
    }


    private async void StartFunction()
    {
        Debug.Log("StartFunction()");
        // await Task.Delay(500);
        await cameraController.FadeIn();

        sceneConfiguration.hpBalanceManagerText.text = gameContext.playerEnemyHpBalance.ToString();

        if (sceneConfiguration.cardsPlayer.Length != 0)
        {
            initializeCardSystem.CreateAndShowCard(2, Side.player,
                sceneConfiguration.cardsPlayer[0]);
        }

        Debug.Log("started iterated levels " + string.Join(',', sceneConfiguration.levels.ToList()));

        foreach (Level level in sceneConfiguration.levels)
        {
            level.levelStartEvents?.Invoke();
            await ProceedLevel(level);
            if (CheckIfGameLost()) return;
            level.levelEndEvents?.Invoke();
        }

        cameraController.ShowGameWon();
    }

    private async Task ProceedLevel(Level level)
    {
        Debug.Log("ProceedLevel()");

        initializeCardSystem.RefreshCardsUIs();

        if (level.dialogObject.IsSet && !level.chooseCardsObject.IsSet && !level.enemyCardsObject.IsSet)
        {
            Debug.Log("dialog level");
            await dialogController.DialogOnlyLevel(level.dialogObject.Value);
        }
        else if (level.dialogObject.IsSet)
        {
            Debug.Log("dialogObject");
            dialogController.DialogLevel(level.dialogObject.Value);
        }

        if (level.chooseCardsObject.IsSet)
        {
            Debug.Log("chooseCardsObject");
            await cardsChoseController.ChooseCardsLevel(level.chooseCardsObject.Value);
        }

        if (level.enemyCardsObject.IsSet)
        {
            Debug.Log("enemyCardsObject Started waiting for player next level");
            await cardsInvasionController.WaitForPlayerClickedNextRound();
            Debug.Log("Player clicked proceed with invasion");
            await cardsInvasionController.CardInvasionLevel(level.enemyCardsObject.Value);
            Debug.Log("Game is finished");
        }
    }

    private bool CheckIfGameLost()
    {
        if (gameContext.playerEnemyHpBalance <= 0)
        {
            cameraController.ShowGameLost("Thanks for playing!");
            return true;
        }

        return false;
    }

    private void Update()
    {
        cameraController.CheckInput();

        if (Input.GetKeyDown(KeyCode.F)
            && sceneConfiguration.monster.isActiveAndEnabled &&
            sceneConfiguration.choosenCardCameraOverlay.isActiveAndEnabled)
        {
            Debug.Log("WIN game!!!!");
            cameraController.ShowGameWon();
        }
    }

    void FixedUpdate()
    {
        _systems?.Run();
    }

    void OnDestroy()
    {
        if (_systems != null)
        {
            _systems.Destroy();
            _systems = null;
            _world.Destroy();
            _world = null;
        }
    }

    // card chosen and taken into the hand
    public void CardChoseUIClickedOnEmptySlot(CardUI cardUI)
    {
        if (shopController.CheckIfCardSkillUpgrade(cardUI))
        {
            return;
        }
        
        gameContext.cardChosenUI = cardUI;
    }

    // player card clicked on a board slot
    public void PlayerCardClickedUI(CardUI cardUI)
    {
        Debug.Log("Player card clicked " + cardUI);

        if (gameContext.cardChosenUI != null
            && gameContext.cardChosenUI.card.itemOnly.IsSet
            && gameContext.cardChosenUI.card.itemOnly.Value.itemAddsSkill.IsSet)
        {
            if (CardsSystem.isDeadOrEmpty(cardUI.card))
            {
                return;
            }

            shopController.AddSkillToACard(gameContext.cardChosenUI, cardUI);
        }
        else
        {
            gameContext.playerCardClickedUI = cardUI;
        }
    }

    public void SeyPlayerClickedNextLevel()
    {
        Debug.Log("Next level clicked");
        sceneConfiguration.clickedNextLevel = true;
    }

    public void BuyCardClicked()
    {
        shopController.BuyCardSlotsClicked();
    }

    // clicked on the inventory card
    public void InventoryClickedOnEmptySlot(CardUI inventoryCardUI)
    {
        Debug.Log("" + inventoryCardUI);
        shopController.InventoryClicked(inventoryCardUI);
    }

    public void SellACardClicked()
    {
        shopController.SellACardClicked(gameContext.cardChosenUI);
        Debug.Log("sell card clicked");
    }

    public void BuyIncomeClicked()
    {
        shopController.BuyIncomeClicked();
    }

    public void SacrificeCardClicked()
    {
        if (sceneConfiguration.choosenCardCameraOverlay == null)
        {
            return;
        }

        if (sceneConfiguration.shop.currentMoney < sceneConfiguration.shop.sacrificeCost)
        {
            return;
        }

        shopController.SacrificeCardClicked(sceneConfiguration.choosenCardCameraOverlay);
        sceneConfiguration.choosenCardCameraOverlay = null;
    }

    public void MonsterReachedPlayer()
    {
        Debug.Log("END game!!!!");
        cameraController.ShowGameLost("You haven't sacrificed a card to the head. Thanks for playing!");
        // end game
    }

    private void test()
    {
        Debug.Log("Start test");
        
        Card card1 = new Card();
        Card card2 = card1.CloneJson();

        card1.IsDead = true;
        card1.hp = 100;
        card1.poisoned = new Optional<Poisoned>();
        card1.poisoned.Value = new Poisoned();
        card1.poisoned.IsSet = true;

        card2.IsDead = false;
        card2.hp = 1337;

        Debug.Log($"card1 {card1}");
        Debug.Log($"card2 {card2}");
    }
}

public static class ObjectCopier
{
    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null)) return default;

        // initialize inner objects individually
        // for example in default constructor some list property initialized with some values,
        // but in 'source' these items are cleaned -
        // without ObjectCreationHandling.Replace default constructor values will be added to result
        var deserializeSettings = new JsonSerializerSettings
            { ObjectCreationHandling = ObjectCreationHandling.Replace };

        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
    }
}


// private void AdvanceLevel()
// {
//     gameContext.currnetLevel += 1;
// }


//
// private async Task<bool> EndTurn()
// {
//     Debug.Log($"New turn playerCards={string.Join(" ,", gameContext.cardsPlayer.ToList())}," +
//               $" enemyCards={string.Join(" ,", gameContext.cardsPlayer.ToList())}");
//     await CardTurnSystem.Instance.CardsTurn(gameContext, sceneConfiguration);
//     if (CardTurnSystem.Instance.CheckForEndGame(gameContext, sceneConfiguration)) return true;
//     return false;
// }
//
// public void DrawCard(Button button)
// {
//     button.gameObject.SetActive(false);
//     CardTurnSystem.Instance.DrawCardInHand(gameContext, sceneConfiguration.handCardsHolder);
//     button.gameObject.SetActive(true);
//     gameContext.needToDraw = false;
// }
//
// private void OnCardUIOnActionCardClicked(Card cardClicked, CardUI cardUiClicked)
// {
//     Debug.Log("Card clicked!");
//
//     if (cardClicked == null && gameContext.cardChosenToDeploy != null &&
//         !CardsChooseUI.Instance.isActiveAndEnabled)
//     {
//         Debug.Log("Init card!");
//         // clicked on slot of a board
//         Card card = CardTurnSystem.Instance.CreateCard(Side.player, gameContext.cardChosenToDeploy.cardObject);
//         CardTurnSystem.Instance.ShowCardData(card, cardUiClicked);
//         gameContext.cardsPlayer[cardUiClicked.cardPosition] = card;
//         CardTurnSystem.Instance.RemoveCardFromHand(gameContext.cardChosenToDeploy, gameContext,
//             sceneConfiguration.handCardsHolder);
//         gameContext.cardChosenToDeploy = null;
//
//         // remove card clicked from deck
//         return;
//     }
// }
//
// public void ShowCardHand(Transform cardsHand)
// {
//     if (!cardsHand.gameObject.activeInHierarchy)
//     {
//         cardsHand.gameObject.SetActive(true);
//     }
//     else
//     {
//         cardsHand.gameObject.SetActive(false);
//     }
// }

//
// if (CardsChooseUI.Instance.isActiveAndEnabled)
// {
//     CardsChooseUI.Instance.gameObject.SetActive(false);
//     gameContext.cardChosenToDeploy = cardClicked;
//     return;
// }
// public void ShowScene(string scene)
// {
//     if (!SceneManager.GetSceneByName(scene).isLoaded)
//     {
//         SceneManager.LoadScene(scene, LoadSceneMode.Additive);
//     }
//     sceneConfiguration.mainSceneHolder.SetActive(false);
// }

//
// public async void EndTurn(Button button)
// {
//     button.gameObject.SetActive(false);
//     // подписать эти вещи на ивент
//     if (await EndTurn()) return;
//     button.gameObject.SetActive(true);
//
//     gameContext.needToDraw = true;
// }
// TableManager.onTableKeyPressed += OnTableManagerTableKeyPressed;
// private void OnTableManagerTableKeyPressed()
// {
//     sceneConfiguration.playerCamera.transform.parent.gameObject.SetActive(false);
//     sceneConfiguration.uiCamera.gameObject.SetActive(true);
//     sceneConfiguration.uiCanvasesPlaceHolder.SetActive(true);
//
//     Cursor.lockState = CursorLockMode.None;
//     Cursor.visible = true;
// }

// private void Update()
// {
//     if (Input.GetKeyDown(KeyCode.Escape) && sceneConfiguration.uiCamera.gameObject.activeSelf 
//                                          && !sceneConfiguration.narrativeImageCanvas.isActiveAndEnabled
//                                          && !sceneConfiguration.cardsCanvas.isActiveAndEnabled)
//     {
//         sceneConfiguration.playerCamera.transform.parent.gameObject.SetActive(true);
//         sceneConfiguration.uiCamera.gameObject.SetActive(false);
//         sceneConfiguration.uiCanvasesPlaceHolder.SetActive(false);
//         
//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Locked;
//     }
//     else if (Input.GetKeyDown(KeyCode.Escape) && sceneConfiguration.uiCamera.gameObject.activeSelf 
//                                          && sceneConfiguration.narrativeImageCanvas.isActiveAndEnabled)
//     {
//         CameraManager.Instance.CloseNarrativeImageScreen();
//     }
//     else if (Input.GetKeyDown(KeyCode.Escape) && sceneConfiguration.uiCamera.gameObject.activeSelf 
//                                          && sceneConfiguration.cardsCanvas.isActiveAndEnabled)
//     {
//         CameraManager.Instance.CloseCardsImageScreen();
//     }
//     
//     if (ga meContext.needToDraw)
//     {
//         sceneConfiguration.drawButton.interactable = true;
//         sceneConfiguration.endTurnButton.interactable = false;
//     }
//     else
//     {
//         sceneConfiguration.drawButton.interactable = false;
//         sceneConfiguration.endTurnButton.interactable = true;
//     }
// }