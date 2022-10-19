using System.Collections.Generic;
using System.Threading.Tasks;
using Client;
using DG.Tweening;
using Leopotam.Ecs;
using MyBox;
using UnityEngine;

public class InitializeCardSystem : IEcsSystem
{
    private SceneConfiguration sceneConfiguration;
    private GameContext gameContext;
    private CardsSystem cardsSystem;

    public async Task InitializeCards(NumAndCard[] cards, Side side)
    {
        // gameContext.cardsPlayer = new Card[sceneConfiguration.CARDS_ON_BOARD_COUNT];
        // gameContext.cardsEnemy = new Card[sceneConfiguration.CARDS_ON_BOARD_COUNT];
        Debug.Log("Intilize cards");

        for (var position = 0; position < cards.Length; position++)
        {
            NumAndCard numAndCard = cards[position];
            for (int i = 0; i < numAndCard.num; i++)
            {
                CardUI card = await CreateAndShowCard(position + i, side, numAndCard.cardObject);
            }
        }

        // Card mainCard = CreateCard(Side.player, sceneConfiguration.mainCardObject);
        // ShowCardData(mainCard, sceneConfiguration.mainBoardCardUI);
    }
    
    /*
    foreach (CardUI cardUI in cardsSystem.GetCardUiList(Side.player))
        {
            cardUI.gameObject.SetActive(true);
            cardUI.view.gameObject.SetActive(true);

            if (cardUI.card == null)
            {
                continue;
            }
            
            Card card = CreateAndShowCard(cardUI.cardPosition, Side.player,
                cardUI.card.cardObject);
        }
     */

    public async Task<CardUI> CreateAndShowCard(CardUI cardUItoPlace, Side side, CardObject cardObject)
    {
        return await CreateAndShowCard(cardUItoPlace.cardPosition,
            side, cardObject);
    }
    
    public async Task<CardUI> CreateAndShowCard(int position,
        Side side, CardObject cardObject)
    {
        Transform holder = side == Side.player
            ? sceneConfiguration.playerCardsHolder
            : sceneConfiguration.enemyCardsHolder;
        
        GameObject appearFromGo = side == Side.player
            ? sceneConfiguration.sceneEffects.playerStartGo
            : sceneConfiguration.sceneEffects.enemyStartGo;
        
        return await CreateAndShowCardInHolder(position, side, cardObject, holder, appearFromGo);
    }

    public void RefreshCardsUIs()
    {
        List<CardUI> initialPlayerCards = cardsSystem.GetCardAllUIs(Side.player);
        List<CardUI> initialEnemyCards = cardsSystem.GetCardAllUIs(Side.enemy);
        foreach (CardUI cardUI in initialPlayerCards)
        {
            cardsSystem.RefreshCard(cardUI);
        }
        
        foreach (CardUI cardUI in initialEnemyCards)
        {
            cardsSystem.RefreshCard(cardUI);
        }
    }

    public async Task<CardUI> CreateAndShowCardInHolder(int position,
        Side side, CardObject cardObject,
        Transform holder, GameObject animationMoveFromGo = null)
    {
        Card card = CreateCard(side, cardObject);
        // card.uIsDead = false;
        CardUI cardUI = ShowCardData(position, card, holder);

        if (animationMoveFromGo != null)
        {
            Vector3 initialPosition = cardUI.transform.position;
            cardUI.transform.position = animationMoveFromGo.transform.position;
            await cardUI.transform.DOMove(initialPosition, 0.25f).AsyncWaitForCompletion();
            cardUI.MoveToStartPosition();
        }

        return cardUI;
    }

    public Card CreateCard(Side side, CardObject cardObject)
    {
        CardObject cloneCardObject = Object.Instantiate(cardObject);
        // cloneCardObject.card = (Card) cloneCardObject.card.Clone();
        
        Card card = cloneCardObject.card;
        card.side = side;
        if (string.IsNullOrEmpty(cloneCardObject.card.name))
        {
            cloneCardObject.card.name = cardObject.name;
        }

        card.cardObject = cloneCardObject;

        return card;
    }
        
    public CardUI ShowCardData(int position, Card card, Transform cardsHolder)
    {
        // if (card.cardObject.card.name.Equals(sceneConfiguration.mainCardObject.card.name))
        // {
        //     Debug.Log("returning " + card.cardObject);
        //     sceneConfiguration.mainBoardCardUI.ShowCardData(card, 0);
        //     return;
        // }

        CardUI cardUI = cardsHolder.GetChild(position).GetComponent<CardUI>();


        if (card == null || card.IsDead)
        {
            Debug.Log("show empty card in " + cardsHolder);
            cardUI.cardPosition = position;
            cardUI.ShowEmptyCardData();
            return cardUI;
        }

        Debug.Log("show normal card in " + cardsHolder + " " + position);

        cardUI.cardPosition = position;
        return ShowCardData(card, cardUI);
    }


    public void MoveCardAndRemove(CardUI cardToMove, CardUI targetCard)
    {
        cardToMove.MoveToStartPosition();
        targetCard.MoveToStartPosition();

        ShowCardData(cardToMove.card, targetCard);
        cardsSystem.RemoveCard(cardToMove);
    }
    
    public void SwapCards(CardUI cardUI1, CardUI cardUI2)
    {
        cardUI1.MoveToStartPosition();
        cardUI2.MoveToStartPosition();
        
        Card buffCard = cardUI1.card;

        ShowCardData(cardUI2.card, cardUI1);
        ShowCardData(buffCard, cardUI2);
    }
    
    public CardUI ShowCardData(Card card, CardUI cardUI)
    {
        cardUI.card = card;
        cardsSystem.RefreshCard(cardUI);
        return cardUI;
    }
}




/*
 *
 *
 * public async Task CardsTurn(GameContext gameContext,
        SceneConfiguration sceneConfiguration)
    {
        for (int position = 0; position < gameContext.cardsPlayer.Length; position++)
        {
            Card card = gameContext.cardsPlayer[position];
            Card enemyCard = gameContext.cardsEnemy[position];

            TakeTurn(card, position,
                gameContext.cardsPlayer, gameContext.cardsEnemy,
                sceneConfiguration.playerCardsHolder,
                sceneConfiguration.enemyCardsHolder, gameContext, sceneConfiguration);

            card = gameContext.cardsPlayer[position];
            enemyCard = gameContext.cardsEnemy[position];

            ShowCardData(position, card, sceneConfiguration.playerCardsHolder);
            ShowCardData(position, enemyCard, sceneConfiguration.enemyCardsHolder);

            if (CheckForEndGame(gameContext, sceneConfiguration)) return;


            if (card != null)
            {
                await Task.Delay(500);
            }
        }

        for (int position = 0; position < gameContext.cardsEnemy.Length; position++)
        {
            Card card = gameContext.cardsEnemy[position];

            TakeTurn(card, position, gameContext.cardsEnemy,
                gameContext.cardsPlayer,
                sceneConfiguration.enemyCardsHolder, sceneConfiguration.playerCardsHolder, gameContext,
                sceneConfiguration);

            if (CheckForEndGame(gameContext, sceneConfiguration)) return;

            await Task.Delay(1000);
        }
    }

    public bool CheckForEndGame()
    {
        if (gameContext.playerEnemyHpBalance >= sceneConfiguration.PLAYER_ENEMY_HP_BALANCE_WIN_TRESHOLD)
        {
            TextPopUpSpawnerManager.Instance.ShowTextPopUpTween("Player win!!!", Color.green,
                sceneConfiguration.playerCardsHolder.transform);
            return true;
        }

        if (gameContext.playerEnemyHpBalance <= -sceneConfiguration.PLAYER_ENEMY_HP_BALANCE_WIN_TRESHOLD)
        {
            TextPopUpSpawnerManager.Instance.ShowTextPopUpTween("Player loose!!!", Color.red,
                sceneConfiguration.playerCardsHolder.transform);
            return true;
        }

        return false;
    }

    public void InitializeCardsForChooseUi(SceneConfiguration sceneConfiguration, GameContext gameContext,
        Transform handCardsHolder)
    {
        gameContext.cardsInDeck = new List<Card>();
        gameContext.cardsInHand = new List<Card>();

        for (var position = 0; position < sceneConfiguration.cardsDeck.Length; position++)
        {
            Card card = CreateCard(Side.player, sceneConfiguration.cardsDeck[position]);
            gameContext.cardsInDeck.Add(card);
        }

        for (int i = 0; i < 3; i++)
        {
            DrawCardInHand(gameContext, handCardsHolder);
        }
    }

    public void DrawCardInHand(GameContext gameContext,
        Transform handCardsHolder)
    {
        if (gameContext.cardsInDeck.Count == 0)
        {
            Debug.Log("Cards in deck count is zero, not drawing");
            return;
        }

        Card card = gameContext.cardsInDeck.GetRandom();
        gameContext.cardsInDeck.Remove(card);
        ShowCardData(gameContext.cardsInHand.Count, card, handCardsHolder);
        gameContext.cardsInHand.Add(card);
    }
    
    private async void TakeTurn(Card card,
        int position,
        Card[] playerSideCards, Card[] enemySideCards, Transform cardsSideHolder,
        Transform enemySideHolder,
        GameContext gameContext, SceneConfiguration sceneConfiguration)
    {
        if (card == null || card.IsDead)
        {
            return;
        }

        if (card.transformation.IsSet)
        {
            Transformation transformation = card.transformation.Value;
            if (transformation.countTurnsToTransform > 0)
            {
                transformation.countTurnsToTransform--;
            }
            else
            {
                card = CreateCard(card.side, transformation.transformTo);
                ShowCardData(position, card, cardsSideHolder);
                // SetCardAtPosition(position, gameContext, card);
            }
        }


        if (position >= enemySideCards.Length)
        {
            // await DamageMainBoard(card, gameContext, sceneConfiguration);
            return;
        }

        Card enemyCard = enemySideCards[position];

        if (enemyCard == null || enemyCard.IsDead)
        {
            // await DamageMainBoard(card, gameContext, sceneConfiguration);
            return;
        }

        CardUI enemyCardUi = enemySideHolder.GetChild(position).GetComponent<CardUI>();
        CardUI playerCardUi = cardsSideHolder.GetChild(position).GetComponent<CardUI>();

        if (enemyCard.shield.IsSet && enemyCard.shield.Value.Alive)
        {
            enemyCard.shield.Value.Alive = false;
            return;
        }

        if (card.poisonOther.IsSet)
        {
            TextPopUpSpawnerManager.Instance.StartTextPopUpTween("Poisoned" + card.damage, Color.red,
                enemyCardUi.transform);
            await Task.Delay(500);
            CardIsDead(position, enemySideCards, enemyCardUi);
            return;
        }

        if (card.damage > 0)
        {
            await DamageCard(card, enemyCard, enemyCardUi, card.damage);

            if (enemyCard.hp <= 0)
            {
                CardIsDead(position, enemySideCards, enemyCardUi);
                return;
            }
        }

        if (enemyCard.quill.IsSet)
        {
            await DamageCard(enemyCard, card, playerCardUi, position);
            if (card.hp <= 0)
            {
                CardIsDead(position, playerSideCards, playerCardUi);
                return;
            }
        }
    }
    
    private static async Task DamageCard(Card card, Card enemyCard, CardUI enemyCardUi, int damage)
    {
        // show in UI damage
        enemyCard.hp -= damage;
        TextPopUpSpawnerManager.Instance.StartTextPopUpTween("-" + damage, Color.red,
            enemyCardUi.transform);
        await Task.Delay(500);
    }

    private static void CardIsDead(int position, Card[] enemySideCards, CardUI carenemyCardUi)
    {
        TextPopUpSpawnerManager.Instance.StartTextPopUpTween("Dead", Color.red,
            carenemyCardUi.transform);
        enemySideCards[position].IsDead = true;

        carenemyCardUi.text.text = "";
        // Debug.Log("Dead!! " + enemySideCards[position]);
    }

    public void RemoveCardFromHand(Card card, GameContext gameContext, Transform cardsHolder)
    {
        gameContext.cardsInHand.Remove(card);
        CardUI[] cardUis = cardsHolder.GetComponentsInChildren<CardUI>();
        for (int position = 0; position < cardUis.Length; position++)
        {
            Card c = position < gameContext.cardsInHand.Count ? gameContext.cardsInHand[position] : null;
            CardUI cardUI = cardUis[position];
            ShowCardData(position, c, cardsHolder);
        }
    }
 */

//
//
// private static void SetCardAtPosition(int position, GameContext gameContext, Card card)
// {
//     if (card.side == Side.player)
//     {
//         gameContext.cardsPlayer[position] = card;
//     }
//     else
//     {
//         gameContext.cardsEnemy[position] = card;
//     }
// }
// using Leopotam.Ecs;
//
// namespace Client
// {
//     internal class CardTurnSystem : IEcsRunSystem
//     {
//         private GameContext gameContext;
//         private EcsFilter<Card, Turn> filter;
//
//         public void Run()
//         {
//             if (!filter.IsEmpty())
//             {
//                 ref EcsEntity cardEntity = ref filter.GetEntity(0);
//                 ref Card card = ref cardEntity.Get<Card>();
//                 int index = gameContext.cardEntitiesEnemy.IndexOf(cardEntity);
//                 
//                 if (card.damage > 0)
//                 {
//                 }
//
//                 if (card.poison.IsSet)
//                 {
//                 }
//
//
//                 if (card.quill.IsSet)
//                 {
//                 }
//
//                 if (card.shield.IsSet)
//                 {
//                 }
//
//                 if (card.shield.IsSet)
//                 {
//                 }
//
//                 cardEntity.Get<TurnEnded>();
//                 cardEntity.Del<Turn>();
//             }
//         }
//     }
// }