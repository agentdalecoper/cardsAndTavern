using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using Leopotam.Ecs;
using UnityEngine;

internal class ShopController : IEcsInitSystem
{
    private CameraController cameraController;
    private CardsChoseController cardsChoseController;
    private InitializeCardSystem initializeCardSystem;
    private CardsSystem cardsSystem;
    private GameContext gameContext;
    private DialogController dialogController;

    private SceneConfiguration sceneConfiguration;

    public static event Action OnMoneyChanged;

    public void Init()
    {
        RefreshShopUis();
    }

    public void BuyCardSlotsClicked()
    {
        Debug.Log("Buy roll clicked " + sceneConfiguration.shop.currentMoney);

        int currentCost = sceneConfiguration.shop.buyCardInitialCost +
                          sceneConfiguration.shop.numberTimesRolled * sceneConfiguration.shop.buyCardCostStep;
        
        if (currentCost > sceneConfiguration.shop.currentMoney)
        {
            return;
        }

        sceneConfiguration.shop.numberTimesRolled += 1;

        cardsChoseController.ChooseCardsLevel();
        RemoveMoney(currentCost);
        RefreshShopUis();
    }
    public void BuyCardClicked(CardUI draggedCard, CardUI underCard)
    {
        if (draggedCard.card.cost > sceneConfiguration.shop.currentMoney)
        {
            return;
        }

        Card boughtCard = draggedCard.card;
        boughtCard.side = Side.player;
        RemoveMoney(boughtCard.cost);

        if (!CheckIfCardSkillUpgrade(draggedCard))
        {
            Debug.Log("Show card data:");
            initializeCardSystem.ShowCardData(draggedCard.card, underCard);
            Debug.Log("remove card:");
            cardsSystem.RemoveCard(draggedCard);
            Debug.Log("MoveToStartPosition:");
            draggedCard.MoveToStartPosition();
        }
    }

    public void BuyIncomeClicked()
    {
        Debug.Log("Buy income clicked");
        if (sceneConfiguration.shop.buyIncomeCost > sceneConfiguration.shop.currentMoney)
        {
            return;
        }

        RemoveMoney(sceneConfiguration.shop.buyIncomeCost);
        sceneConfiguration.shop.currentIncome +=
            sceneConfiguration.shop.currentIncome + gameContext.invasionLevel;

        sceneConfiguration.shop.buyIncomeCost +=
            sceneConfiguration.shop.buyIncomeCost + gameContext.invasionLevel;

        RefreshShopUis();
    }

    public void AddMoney(int money)
    {
        sceneConfiguration.shop.currentMoney += money;
        RefreshShopUis();
        OnMoneyChanged?.Invoke();
        MoneyDropManager.Instance.DropCoins(money);
    }

    public void RemoveMoney(int money)
    {
        sceneConfiguration.shop.currentMoney -= money;
        RefreshShopUis();
        OnMoneyChanged?.Invoke();
        MoneyDropManager.Instance.RemoveCoins(money);
    }

    public void RefreshShopUis()
    {
        sceneConfiguration.shop.currentMoneyUI.text
            = sceneConfiguration.shop.currentMoney + "$";

        sceneConfiguration.shop.buyCardUI.costText.text
            = sceneConfiguration.shop.buyCardCostStep + "$";

        if (sceneConfiguration.shop.buyCardCostStep > sceneConfiguration.shop.currentMoney)
        {
            sceneConfiguration.shop.buyCardUI.costText.color = Color.grey;
        }


        sceneConfiguration.shop.buyIncomeUI.costText.text
            = sceneConfiguration.shop.buyIncomeCost + "$";

        if (sceneConfiguration.shop.buyIncomeCost > sceneConfiguration.shop.currentMoney)
        {
            sceneConfiguration.shop.buyIncomeUI.costText.color = Color.grey;
        }

        int currentRollCost = sceneConfiguration.shop.buyCardInitialCost +
                              sceneConfiguration.shop.numberTimesRolled * sceneConfiguration.shop.buyCardCostStep;
        sceneConfiguration.shop
            .rollACardHolder.GetComponentInChildren<TextMesh>().text = "Roll " + currentRollCost + "$";
    }

    public void SacrificeCardClicked(CardUI cardToSacrificeUI)
    {
        cardToSacrificeUI.card = null;
    }

    public async void InventoryClicked(CardUI inventoryCardUI)
    {
        if (!CardsSystem.isDeadOrEmpty(inventoryCardUI.card))
        {
            await cardsChoseController.TakeCardInHand(inventoryCardUI);
            cardsSystem.RemoveCard(inventoryCardUI);
            gameContext.cardChosenUI = sceneConfiguration.choosenCardCameraOverlay;

            return;
        }

        if (gameContext.cardChosenUI == null
            || CardsSystem.isDeadOrEmpty(gameContext.cardChosenUI.card))
        {
            return;
        }

        Card card = gameContext.cardChosenUI.card;

        // await cardsChoseController.AnimationFromHandToTable(
        //     sceneConfiguration.choosenCardCameraOverlay, inventoryCardUI);
        cardsChoseController.NullifyUIs();

        initializeCardSystem.CreateAndShowCardInHolder(
            inventoryCardUI.cardPosition,
            Side.player,
            card.cardObject,
            sceneConfiguration.shop.inventoryCardsHolder.transform);

        gameContext.cardChosenUI = null;
    }

    public List<CardUI> GetInventoryUICards()
    {
        return sceneConfiguration.shop.inventoryCardsHolder
            .GetComponentsInChildren<CardUI>().ToList();
    }

    public void SellACardClicked(CardUI gameContextCardChosenUI)
    {
        Card card = gameContextCardChosenUI.card;
        cardsSystem.RemoveCard(gameContextCardChosenUI);
        AddMoney(card.cost);
        cardsChoseController.NullifyUIs();
        gameContextCardChosenUI.MoveToStartPosition();
    }

    public void ProcessLevelEndedIncome(bool levelWon, int levelLevelIncome)
    {
        int levelWonMoney = levelWon ? 1 : 0;
        int incomeMoney = sceneConfiguration.shop.currentMoney / 5;
        var incomeFromItems = GetInventoryUICards()
            .Union(cardsSystem.GetCardAllUIs(Side.player))
            .Where(c => !CardsSystem.isDeadOrEmpty(c.card))
            .Select(c => c.card)
            .Where(c => c.itemOnly.IsSet
                        && c.itemOnly.Value.income.IsSet)
            .Select(c => c.itemOnly.Value.income.Value.income)
            .Sum();

        string incomeFromItemsText = +incomeFromItems == 0 ? "" : $", income from workers={incomeFromItems}";
        AddMoney(incomeMoney + levelWonMoney + incomeFromItems + levelLevelIncome);

        DialogTextManager.Instance.ShowText(
            $"Income added income from level={levelLevelIncome}," +
            $"income={incomeMoney}"
            + incomeFromItemsText);
    }

    public void AddSkillToACard(CardUI itemCardWithSkill, CardUI cardSkillToAdd)
    {
        itemCardWithSkill.MoveToStartPosition();
        cardSkillToAdd.MoveToStartPosition();

        SkillObject skillObject = cardsSystem
            .GetActiveSkillObjects(itemCardWithSkill.card.itemOnly.Value.itemAddsSkill.Value.cardWithSkill)
            .First();

        cardsSystem.AddSkillToACard(cardSkillToAdd, skillObject);
        cardsSystem.RemoveCard(itemCardWithSkill);
        cardsChoseController.NullifyUIs();
    }

    public bool CheckIfCardSkillUpgrade(CardUI cardUI)
    {
        List<CardUI> cardUIsInTheInventory = GetInventoryUICards();
        List<CardUI> cardUIsOnTheBoard = cardsSystem.GetCardAllUIs(Side.player);

        List<CardUI> sameInventoryCards =
            cardUIsInTheInventory.Where(c => !CardsSystem.isDeadOrEmpty(c.card)
                                             && cardUI.card.cardObject.name == c.card.cardObject.name).ToList();
        List<CardUI> sameBoardCards = cardUIsOnTheBoard.Where(c => !CardsSystem.isDeadOrEmpty(c.card)
                                                                   && cardUI.card.cardObject.name == c.card.cardObject.name).ToList();

        Debug.Log($"Checking card skill upgarde" +
                  $" sameInventoryCards={string.Join(",", sameInventoryCards)} sameBoardCards={string.Join(",", sameBoardCards)}");
        if (sameInventoryCards.Count + sameBoardCards.Count >= 2)
        {
            List<CardUI> sameCardsUnion = sameBoardCards.Union(sameInventoryCards).ToList();

            CardUI mainCardToStarInto = sameCardsUnion.First();
            sameCardsUnion.Remove(mainCardToStarInto);

            CardUI secondCard = sameCardsUnion.First();
            sameCardsUnion.Remove(mainCardToStarInto);

            CardUI thirdCard = cardUI;

            StarUpgrade(mainCardToStarInto);
            cardsSystem.RemoveCard(secondCard);
            cardsSystem.RemoveCard(thirdCard);
            
            mainCardToStarInto.MoveToStartPosition();
            secondCard.MoveToStartPosition();
            thirdCard.MoveToStartPosition();

            return true;
        }

        return false;
    }

    private void StarUpgrade(CardUI cardToStarUpgrade)
    {
        Card upgradedCard = initializeCardSystem.CreateCard(Side.player,
            cardToStarUpgrade.card.cardObject.nextGrade);
        cardToStarUpgrade.card = upgradedCard;

        cardsSystem.RefreshCard(cardToStarUpgrade);
        // cameraController.ShowShopAndPlayerCards();
        // damage / hp x2? 
    }
}