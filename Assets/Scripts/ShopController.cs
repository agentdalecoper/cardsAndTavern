using Client;
using Leopotam.Ecs;
using UnityEngine;

internal class ShopController : IEcsInitSystem
{
    private CameraController cameraController;
    private CardsChoseController cardsChoseController;
    private GameContext gameContext;

    private SceneConfiguration sceneConfiguration;

    public void Init()
    {
        RefreshShopUis();
    }

    public void BuyCardClicked()
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
}