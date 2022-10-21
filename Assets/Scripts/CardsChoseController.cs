using System.Threading.Tasks;
using Client;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Leopotam.Ecs;
using MyBox;
using UnityEngine;

public class CardsChoseController : IEcsSystem
{
    private SceneConfiguration sceneConfiguration;
    private GameContext gameContext;
    private InitializeCardSystem initializeCardSystem;
    private CardsSystem cardsSystem;
    private CameraController cameraController;

    private CardUI emptyCardSlotClicked;

    public void ChooseCardsLevel()
    {
        ChoseCardsObject shopChoseCardsObject = sceneConfiguration.shop.shopChoseCardsObject;
        ChooseCardsLevel(new[]
        {
            shopChoseCardsObject.cardsToChoseFrom.GetRandom(),
            shopChoseCardsObject.cardsToChoseFrom.GetRandom()
        });
    }

    public async UniTask ChooseCardsLevel(ChoseCardsObject choseCardsObject)
    {
        //CardObject[] cardsToChoseFrom = choseCardsObject.cardsToChoseFrom;
        
        ChoseCardsObject shopChoseCardsObject = sceneConfiguration.shop.shopChoseCardsObject;

        await ChooseCardsLevel(new[]
        {
            shopChoseCardsObject.cardsToChoseFrom.GetRandom(),
            shopChoseCardsObject.cardsToChoseFrom.GetRandom()
        });

        Debug.Log("Finished chose cards level");
    }

    public async UniTask ChooseCardsLevel(CardObject[] cardsToChoseFrom)
    {
        gameContext.isCardChoseLevel = true;
        
        // NullifyUIs();
        ShowChoseCardsUi(cardsToChoseFrom);
        // CardUI chosenCardUi = await CheckForCardChosen();
        // await TakeCardInHandAndWaitForClick(chosenCardUi);
        
        gameContext.isCardChoseLevel = false;
    }

    public async UniTask TakeCardInHandAndWaitForClick(CardUI chosenCardUi)
    {
        await TakeCardInHand(chosenCardUi);
        CardUI emptySlotCardUi = await CheckForEmptySlotClicked();
        
        initializeCardSystem.ShowCardData(emptySlotCardUi.cardPosition,
            sceneConfiguration.choosenCardCameraOverlay.card, 
            sceneConfiguration.playerCardsHolder);

        NullifyUIs();
    }

    public async UniTask TakeCardInHand(CardUI chosenCardUi)
    {
        // await AnimationCardFromTableToHand(chosenCardUi);
        RefreshCameraOverlayCard(chosenCardUi);
        cameraController.ShowChosenOverlayCard();
        cameraController.ShowShopAndPlayerCards();
    }

    private void RefreshCameraOverlayCard(CardUI chosenCardUi)
    {
        cardsSystem.RefreshCard(chosenCardUi);
        sceneConfiguration.choosenCardCameraOverlay.card = chosenCardUi.card;
        cardsSystem.RefreshCard(sceneConfiguration.choosenCardCameraOverlay);
    }

    public void NullifyUIs()
    {
        gameContext.cardChosenUI = null;
        gameContext.playerCardClickedUI = null;
        sceneConfiguration.choosenCardCameraOverlay.gameObject.SetActive(false);
    }

    public async UniTask<CardUI> CheckForEmptySlotClicked()
    {
        Debug.Log("check for empty slot clicked");
        while (gameContext.cardChosenUI == null ||
               gameContext.playerCardClickedUI == null || gameContext.playerCardClickedUI.card != null)
        {
            if (gameContext.cardChosenUI != null && gameContext.cardChosenUI.card != null
                                                 && gameContext.cardChosenUI.card.itemOnly.IsSet
                                                 && gameContext.cardChosenUI.card.itemOnly.Value.itemAddsSkill.IsSet)

                Debug.Log("Wait for empty slot clicked");
            await UniTask.Yield();
        }

        // await AnimationFromHandToTable(sceneConfiguration.choosenCardCameraOverlay, 
        //     gameContext.playerCardClickedUI);

        Debug.Log("waiting started suc card is " + gameContext.playerCardClickedUI.card);

        return gameContext.playerCardClickedUI;
    }

    public async UniTask AnimationFromHandToTable(CardUI handCard, CardUI toCard)
    {
        var cardOverlayTransform = handCard.transform;
        var initialPos = cardOverlayTransform.position;
        var initialRotation = cardOverlayTransform.rotation;
        var initialLocalScale = cardOverlayTransform.localScale;

        var targetTableCardTransform = toCard.transform;
        Debug.Log($"scale from {cardOverlayTransform.localScale} to {targetTableCardTransform.localScale}");

        var tween = DOTween.Sequence().Join(cardOverlayTransform.DOMove(targetTableCardTransform.position, 1f))
            .Join(cardOverlayTransform.DORotateQuaternion(targetTableCardTransform.rotation, 2f))
            // .Join(cardOverlayTransform.DOScale(targetTableCardTransform.localScale, 2f))
            ;

        await tween.AsyncWaitForCompletion();

        cardOverlayTransform.position = initialPos;
        cardOverlayTransform.rotation = initialRotation;
        // cardOverlayTransform.localScale = initialLocalScale;
    }

    private async UniTask<CardUI> CheckForCardChosen()
    {
        Debug.Log("check for card chosen");
        while (gameContext.cardChosenUI == null)
        {
            Debug.Log("Wait for card chosen");
            await UniTask.Yield();
        }
        
        return gameContext.cardChosenUI;
    }

    private async UniTask AnimationCardFromTableToHand(CardUI chosenCardUi)
    {
        var cardChosenOnBoard = chosenCardUi.transform;
        var targetCardOverlayTransform = sceneConfiguration.choosenCardCameraOverlay.transform;

        var initialPos = cardChosenOnBoard.position;
        var initialRotation = cardChosenOnBoard.rotation;
        var initialLocalScale = cardChosenOnBoard.localScale;

        var tween = DOTween.Sequence()
            .Join(cardChosenOnBoard.DOMove(targetCardOverlayTransform.position, 1f))
            .Join(cardChosenOnBoard.DORotateQuaternion(targetCardOverlayTransform.rotation, 1f))
            // .Join(cardChosenOnBoard.DOScale(targetCardOverlayTransform.localScale, 2f))
            ;

        await tween.AsyncWaitForCompletion();

        cardChosenOnBoard.position = initialPos;
        cardChosenOnBoard.rotation = initialRotation;
        // cardChosenOnBoard.localScale = initialLocalScale;

    }

    private void SubscribeToCardsChosenEvent()
    {
        foreach (CardUI cardChoseUi in sceneConfiguration.cardsChooseHolder.GetComponentsInChildren<CardUI>())
        {
            // cardChoseUi.ActionCardClicked += (card, ui) =>
            // {
            //     OnCardChoseUiOnActionCardClicked(card, ui, cardChoseUi);
            // };
        }
    }

    public void ShowChoseCardsUi(CardObject[] cardsToChoseFrom)
    {
        initializeCardSystem.CreateAndShowCardInHolder(
            0, Side.shop, cardsToChoseFrom[0],
            sceneConfiguration.cardsChooseHolder,
            sceneConfiguration.sceneEffects.inventoryStartGo);
        initializeCardSystem.CreateAndShowCardInHolder(
            1, Side.shop, cardsToChoseFrom[1],
            sceneConfiguration.cardsChooseHolder,
            sceneConfiguration.sceneEffects.inventoryStartGo);
        // initializeCardSystem.CreateAndShowCardInHolder(2, Side.player, cardsToChoseFrom[2],
        //     sceneConfiguration.cardsChooseHolder);

        // sceneConfiguration.playerCardsHolder.parent.gameObject.SetActive(false);
        sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);
        
        Debug.Log("Show cards chose");
        // cameraController.ShowTable();
    }
}