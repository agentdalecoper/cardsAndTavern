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

    public void Init()
    {
        RefreshShopUis();
    }

    public void BuyCardSlotsClicked()
    {
        Debug.Log("Buy card clicked");

        if (sceneConfiguration.shop.buyCardCost > sceneConfiguration.shop.currentMoney)
        {
            return;
        }

        cardsChoseController.ChooseCardsLevel();
        RemoveMoney(sceneConfiguration.shop.buyCardCost);
        RefreshShopUis();
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
    }

    public void RemoveMoney(int money)
    {
        sceneConfiguration.shop.currentMoney -= money;
        RefreshShopUis();
    }

    private void RefreshShopUis()
    {
        sceneConfiguration.shop.currentMoneyUI.text
            = sceneConfiguration.shop.currentMoney + "$";

        sceneConfiguration.shop.buyCardUI.costText.text
            = sceneConfiguration.shop.buyCardCost + "$";

        if (sceneConfiguration.shop.buyCardCost > sceneConfiguration.shop.currentMoney)
        {
            sceneConfiguration.shop.buyCardUI.costText.color = Color.grey;
        }


        sceneConfiguration.shop.buyIncomeUI.costText.text
            = sceneConfiguration.shop.buyIncomeCost + "$";

        if (sceneConfiguration.shop.buyIncomeCost > sceneConfiguration.shop.currentMoney)
        {
            sceneConfiguration.shop.buyIncomeUI.costText.color = Color.grey;
        }
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

        await cardsChoseController.AnimationFromHandToTable(
            sceneConfiguration.choosenCardCameraOverlay, inventoryCardUI);
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

    public void SellACardClicked()
    {
        if (gameContext.cardChosenUI == null
            || CardsSystem.isDeadOrEmpty(gameContext.cardChosenUI.card))
        {
            return;
        }

        Card card = gameContext.cardChosenUI.card;


        AddMoney(card.cost);
        gameContext.cardChosenUI = null;
    }

    public void ProcessLevelEndedIncome(bool levelWon)
    {
        int levelWonMoney = levelWon ? 2 : 0;
        int incomeMoney = sceneConfiguration.shop.currentMoney / 10;
        var incomeFromItems = GetInventoryUICards()
            .Where(c => !CardsSystem.isDeadOrEmpty(c.card))
            .Select(c => c.card)
            .Where(c => c.itemOnly.IsSet
                        && c.itemOnly.Value.income.IsSet)
            .Select(c => c.itemOnly.Value.income.Value.income)
            .Sum();

        string incomeFromItemsText = +incomeFromItems == 0 ? "" : $", income from items={incomeFromItems}";
        AddMoney(incomeMoney + levelWonMoney + incomeFromItems);

        DialogTextManager.Instance.ShowText($"Income added level money = {levelWonMoney}, " +
                                            $"income={incomeMoney}"
                                            + incomeFromItemsText);
    }

    public void AddSkillToACard(CardUI itemCardWithSkill, CardUI cardSkillToAdd)
    {

        SkillObject skillObject = cardsSystem
            .GetActiveSkillObjects(itemCardWithSkill.card.itemOnly.Value.itemAddsSkill.Value.cardWithSkill)
            .First();
        
        cardsSystem.AddSkillToACard(cardSkillToAdd, skillObject);
        cardsChoseController.NullifyUIs();
    }

    public bool CheckIfCardSkillUpgrade(CardUI cardUI)
    {
        List<CardUI> cardUIsInTheInventory = GetInventoryUICards();
        List<CardUI> cardUIsOnTheBoard = cardsSystem.GetCardUIList(Side.player);

        List<CardUI> sameInventoryCards =
            cardUIsInTheInventory.Where(c => !CardsSystem.isDeadOrEmpty(c.card)
                                             && cardUI.card.name == c.card.name).ToList();
        List<CardUI> sameBoardCards = cardUIsOnTheBoard.Where(c => !CardsSystem.isDeadOrEmpty(c.card)
                                                                   && cardUI.card.name == c.card.name).ToList();

        if (sameInventoryCards.Count + sameBoardCards.Count >= 3)
        {
            CardUI mainCardToStarInto = sameInventoryCards.First();
            sameInventoryCards.Remove(mainCardToStarInto);
            CardUI secondCard = cardUI;
            CardUI thirdCard = sameInventoryCards.Union(sameBoardCards).First();

            StarUpgrade(mainCardToStarInto);
            cardsSystem.RemoveCard(secondCard);
            cardsSystem.RemoveCard(thirdCard);

            return true;
        }

        return false;
    }

    private void StarUpgrade(CardUI cardToStarUpgrade)
    {
        Card card = cardToStarUpgrade.card;
        card.numberOfSlots++;
        // damage / hp x2? 
    }
}