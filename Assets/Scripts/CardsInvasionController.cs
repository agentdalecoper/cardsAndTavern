using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Leopotam.Ecs;
using UnityEngine;

namespace Client
{
    public class CardsInvasionController : IEcsSystem, IEcsInitSystem
    {
        private SceneConfiguration sceneConfiguration;
        private GameContext gameContext;
        private InitializeCardSystem initializeCardSystem;
        private CardsSystem cardsSystem;
        private CameraController cameraController;
        private CardsChoseController cardsChoseController;
        private ShopController shopController;

        public void Init()
        {
            AssignCardUIPositions(Side.player);
            AssignCardUIPositions(Side.enemy);
            AssignInventoryCardPositions();
        }
        
        /*
         *
         * enemy from left to right choses ->
         * first card tries to choose firs in front
         *         not found => search left to the leftmost
         *         not found => search right to the rightmost
         */
        public async Task CardInvasionLevel(EnemyCardsObject enemyCardsObject)
        {
            initializeCardSystem.RefreshCardsUIs();

            Card[] initialPlayerCards = cardsSystem
                .GetCardList(Side.player)
                .Select(CloneCard)
                .ToArray();
            // Card[] initialEnemyCards = cardsSystem.GetCardList(Side.enemy).Select(CloneCard).ToArray();

            InitiateDialogAndCardsInvasion(enemyCardsObject);
            
            // CardUI.ActionCardDraggedOn += (ui, cardUI) => Turn(ui, cardUI);
            
            bool levelWon = await CheckCardSessionIsFinished();
            PlacePlayerCardsAgain(initialPlayerCards);
            cameraController.ShowPlayerOnlyCards();

            shopController.ProcessLevelEndedIncome(levelWon);

            // CardUI.ActionCardDraggedOn -= (ui, cardUI) =>  Turn(ui, cardUI);
        }

        private Card CloneCard(Card toCloneCard)
        {
            if (toCloneCard == null)
            {
                return null;
            }
            
            Card result =  toCloneCard.CloneJson();
            result.sprite = toCloneCard.sprite;
            result.cardObject = toCloneCard.cardObject;
            result.side = toCloneCard.side;
            if (toCloneCard.transformation.IsSet)
            {
                result.transformation.Value.transformTo
                    = toCloneCard.transformation.Value.transformTo;
            }
            
            return result;
        }

        private void PlacePlayerCardsAgain(Card[] initialPlayerCards)
        {
            Debug.Log("Cards saved for player before: " + string.Join(",",initialPlayerCards.ToList()));

            for (var i = 0; i < initialPlayerCards.Length; i++)
            {
                Card card = initialPlayerCards[i];
                CardUI cardUI;
                
                if (card != null)
                {
                    cardUI = cardsSystem.GetCardAllUIs(Side.player)[i];
                    cardUI.card = card;
                    cardsSystem.RefreshCard(cardUI);
                    cardUI.gameObject.SetActive(true);
                }
                else
                {
                    cardUI = cardsSystem.GetCardAllUIs(Side.player)[i];
                    cardsSystem.RemoveCard(cardUI);
                    Debug.Log("showed empty ui: " + cardUI);
                }
            }
        }

        private async Task<bool> CheckCardSessionIsFinished()
        {
            while (!cardsSystem.CheckDamageBoardOrNoEnemy())
            {
                await cardsSystem.IterateCardsAndDamage();
                Debug.Log("Iterated cards and waiting for end session");
                await Task.Yield();
            }

            return await cardsSystem.EndOfInvasion();
        }

        public void AssignCardUIPositions(Side side)
        {
            List<CardUI> uis = cardsSystem
                .GetCardHolder(side)
                .GetComponentsInChildren<CardUI>()
                .ToList();

            for (var i = 0; i < uis.Count; i++)
            {
                CardUI cardUI = uis[i];
                cardUI.cardPosition = i;
            }
        }

        public void AssignInventoryCardPositions()
        {
            List<CardUI> uis = shopController.GetInventoryUICards();
            for (var i = 0; i < uis.Count; i++)
            {
                CardUI cardUI = uis[i];
                cardUI.cardPosition = i;
            }
        }

        private void InitiateDialogAndCardsInvasion(EnemyCardsObject enemyCardsObject)
        {
            initializeCardSystem.InitializeCards(
                enemyCardsObject.cardsEnemy, Side.enemy);
            
            cameraController.ShowInvasionAndPlayerCards();
        }

        public async Task WaitForPlayerClickedNextRound()
        {
            while (!sceneConfiguration.clickedNextLevel)
            {
                if (gameContext.playerCardClickedUI != null
                    && gameContext.cardChosenUI == null &&
                    !gameContext.isCardChoseLevel)
                {
                    await TakeCardIntoAHand(gameContext.playerCardClickedUI);
                    gameContext.playerCardClickedUI = null;
                    CardUI emptySlotCardUi = await cardsChoseController.CheckForEmptySlotClicked();
                    Debug.Log("Create and show card + " + sceneConfiguration.choosenCardCameraOverlay.card);
                    initializeCardSystem.ShowCardData(emptySlotCardUi.cardPosition,
                        sceneConfiguration.choosenCardCameraOverlay.card,
                        sceneConfiguration.playerCardsHolder);

                    cardsChoseController.NullifyUIs();
                }
                
                Debug.Log("Waiting for level clicked next round");
                await Task.Yield();
            }

            sceneConfiguration.clickedNextLevel = false;
        }

        private async Task TakeCardIntoAHand(CardUI cardUI)
        {
            gameContext.cardChosenUI = cardUI;
            await cardsChoseController.TakeCardInHand(cardUI);
            sceneConfiguration.choosenCardCameraOverlay.card = CloneCard(cardUI.card);
            Debug.Log("result cloned card " + sceneConfiguration.choosenCardCameraOverlay.card);
            cardUI.card = null;
            gameContext.cardChosenUI = sceneConfiguration.choosenCardCameraOverlay;
            gameContext.playerCardClickedUI = null;
            cardsSystem.RefreshCard(cardUI);
        }
    }
}