using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Leopotam.Ecs;
using MyBox;
using UnityEngine;

namespace Client
{
    public class CardsSystem : IEcsSystem
    {
        private SceneConfiguration sceneConfiguration;
        private GameContext gameContext;
        private InitializeCardSystem initializeCardSystem;
        private CameraController cameraController;
        private ShopController shopController;

        public async Task CardTurn(CardUI cardUi, CardUI enemyCardUi)
        {
            Card card = cardUi.card;
            Card enemyCard = enemyCardUi.card;

            if (isDead(card)) return;

            if (isDead(enemyCard)) return;

            await CardTurn(enemyCard, card, enemyCardUi, cardUi);
            await CardTurn(card, enemyCard, cardUi, enemyCardUi);

            initializeCardSystem.ShowCardData(cardUi.cardPosition, card,
                sceneConfiguration.enemyCardsHolder);
            initializeCardSystem.ShowCardData(enemyCardUi.cardPosition, enemyCard,
                sceneConfiguration.playerCardsHolder);
        }

        public async Task CardsTurn(CardUI cardUI, CardUI enemyCardUI, Side side)
        {
            foreach (CardUI crdUi in GetCardUiList(side))
            {
                Card crd = crdUi.card;

                if (crdUi.card == null || crdUi.card.IsDead)
                {
                    continue;
                }

                if (crdUi.card.arrowShot.IsSet && crdUi.card != cardUI.card)
                {
                    await ArrowShot(crdUi);
                }

                if (crd.poisoned.IsSet)
                {
                    await CheckDamageWithPoison(crdUi, crd);
                }

                if (crd.transformation.IsSet)
                {
                    Transformation transformation = crd.transformation.Value;
                    if (transformation.countTurnsToTransform > 0)
                    {
                        transformation.countTurnsToTransform--;
                    }
                    else
                    {
                        await AnimateTransformShake(crdUi);
                        Card transformedCard = initializeCardSystem.CreateCard(crd.side, transformation.transformTo);
                        initializeCardSystem.ShowCardData(crdUi.cardPosition, transformedCard, GetCardHolder(crd.side));
                        
                        // crdUi.cardFace.color = Color.red;
                        // initializeCardSystem.SetCardAtPosition(crdUi.cardPosition, gameContext, crd);
                    }
                }
            }
        }

        public bool CheckDamageBoardOrNoEnemy()
        {
            List<CardUI> aliveCards = GetCardUiList(Side.player)
                .Where(c => !isDead(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardUiList(Side.enemy)
                .Where(c => !isDead(c.card))
                .ToList();

            // Debug.Log($"Alive cards count player {aliveCards.Count}, alive cards enemy count {aliveCardsEnemy.Count}");
            if (aliveCards.Count == 0 && aliveCardsEnemy.Count != 0)
            {
                return true;
            }

            if (aliveCardsEnemy.Count == 0)
            {
                return true;
            }

            return false;
        }
        
        public async Task IterateCardsAndDamage()
        {
            foreach (CardUI enemyCardUI in GetCardUiList(Side.enemy))
            {
                if (enemyCardUI == null || isDead(enemyCardUI.card))
                {
                    continue;
                }

                CardUI playerCardUI = GetCardToAttack(enemyCardUI);
                if (playerCardUI != null && !isDead(playerCardUI.card))
                {
                    await Turn(enemyCardUI, playerCardUI);
                }
                else
                {
                    break;
                }
            }
        }
        
        private CardUI GetCardToAttack(CardUI enemyCardUI)
        {
            CardUI playerCardUI = GetCardAcross(enemyCardUI);
            if (playerCardUI == null || isDead(playerCardUI.card))
            {
                for (int i = 0; i < sceneConfiguration.CARDS_ON_BOARD_COUNT; i++)
                {
                    playerCardUI = GetCardAcrossAtPosition(enemyCardUI, i);
                    Debug.Log("Found player card " + playerCardUI);

                    if (playerCardUI != null && !isDead(playerCardUI.card))
                    {
                        return playerCardUI;
                    }
                }
            }

            return playerCardUI;
        }
        
        public async Task Turn(CardUI enemyCardUi, CardUI playerCardUi)
        {
            // await Task.k(100);
            Debug.Log(" local pos player hold " + playerCardUi.transform.localPosition);
            await enemyCardUi.transform.DOPunchPosition((playerCardUi.transform.localPosition)
                                                        - enemyCardUi.transform.localPosition 
                                                        
                , 0.5f,1).AsyncWaitForCompletion();
            
            await CardTurn(enemyCardUi,
                playerCardUi);
            
            await CardsTurn(enemyCardUi,
                playerCardUi,  Side.player);

            await CardsTurn(enemyCardUi,
                playerCardUi, Side.enemy);
        }

        private static async Task AnimateTransformShake(CardUI playerCardUi)
        {
            var localRotation = playerCardUi.transform.localRotation;
            await playerCardUi.transform.DOShakeScale(0.5f, 0.5f).AsyncWaitForCompletion();
            playerCardUi.transform.localRotation = localRotation;
        }

        private static async Task AnimateDamageShake(CardUI playerCardUi)
        {
            var localRotation = playerCardUi.transform.localRotation;
            await playerCardUi.transform.DOShakeRotation(0.5f, 10f).AsyncWaitForCompletion();
            playerCardUi.transform.localRotation = localRotation;
        }


        public async Task EndOfInvasion()
        {
            List<CardUI> aliveCards = GetCardUiList(Side.player)
                .Where(c => !isDead(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardUiList(Side.enemy)
                .Where(c => !isDead(c.card))
                .ToList();

            int incomeMoney = aliveCards.Count
                              + gameContext.invasionLevel;
            
            if (aliveCards.Count == 0 && aliveCardsEnemy.Count != 0)
            {
                int damage = 0;

                foreach (CardUI cardUI in aliveCardsEnemy)
                {
                    damage += cardUI.card.damage;
                }

                cameraController.ShowLeftward();
                await cameraController.AwaitCinemachineBlending();
                gameContext.playerEnemyHpBalance -= damage;
                Debug.Log("Damaging main board with damage " + damage);
                // await Task.Delay(300);
                await AnimationDamageMainBoard();
                cameraController.ShowTable();
            }
            else
            {
                incomeMoney += sceneConfiguration
                    .levels[gameContext.invasionLevel]
                    .enemyCardsObject.Value.cardsEnemy.Length;
            }

            shopController.AddMoney(incomeMoney);

            gameContext.invasionLevel++;
        }

        private async Task AnimationDamageMainBoard()
        {
            sceneConfiguration.hpBalanceManagerText.text = gameContext.playerEnemyHpBalance.ToString();
            var tw1 = DOTween.To(() =>
                    sceneConfiguration.hpBalanceManagerText.color,
                x => sceneConfiguration.hpBalanceManagerText.color = x,
                Color.red, 1f);
            var tw2 =
                sceneConfiguration.hpBalanceManagerText.transform.DOShakeScale(1f, 2f);

            await DOTween.Sequence().Join(tw1).Join(tw2).AsyncWaitForCompletion();
        }


        private async Task CheckDamageWithPoison(CardUI cardUI, Card card)
        {
            Debug.Log("check damage with poison");

            Poisoned poisoned = card.poisoned.Value;

            // if (poisoned.needToTick)
            // {
            //     poisoned.needToTick = false;
            // }
            // else
            // {
            await DamageCardDirectly(card, cardUI, 1);
            // card.poisoned.Value = null;
            // card.poisoned.IsSet = false;
            // }

            RefreshCard(cardUI);
        }

        private async Task ArrowShot(CardUI playerCardUI)
        {
            CardUI acrossEnemyCardUI =
                GetCardAcross(playerCardUI);
            await DamageCardDirectly(acrossEnemyCardUI.card, acrossEnemyCardUI, 1);
            Debug.Log($"Arrow shot enemy card {acrossEnemyCardUI} player card {playerCardUI}");
        }

        private async Task CardTurn(Card card, Card enemyCard, CardUI cardUi, CardUI enemyCardUi)
        {
            if (card.damage > 0)
            {
                await DamageCard(card, enemyCard, cardUi, enemyCardUi);
            }

            if (card.splitAttack.IsSet)
            {
                await SplitAttack(card, enemyCardUi);
            }

            if (card.gyroAttack.IsSet)
            {
                await GyroAttack(card);
            }
        }

        private async Task GyroAttack(Card card)
        {
            Debug.Log("gyro attack " + card);

            List<CardUI> cardsAcross = GetCardUiListAcross(card);
            List<CardUI> aliveCardsAcross = cardsAcross.Where(c => c.card != null && c.card.IsDead == false).ToList();

            if (aliveCardsAcross.Count > 0)
            {
                CardUI acrossCardUI1 = aliveCardsAcross.GetRandom();
                await DamageCardDirectly(acrossCardUI1.card, acrossCardUI1, 1);
                aliveCardsAcross.Remove(acrossCardUI1);
            }

            if (aliveCardsAcross.Count > 0)
            {
                CardUI acrossCardUI = aliveCardsAcross.GetRandom();
                await DamageCardDirectly(acrossCardUI.card, acrossCardUI, 1);
                aliveCardsAcross.Remove(acrossCardUI);
            }
        }

        private async Task SplitAttack(Card card, CardUI enemyCardUi)
        {
            Debug.Log("split attack " + card);
            List<CardUI> cardsAcross = GetCardUiListAcross(card);

            if (enemyCardUi.cardPosition - 1 >= 0)
            {
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition - 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi, 1);
                Debug.Log("split attack left " + cardAcrossUi.card);
            }

            if (enemyCardUi.cardPosition + 1 < sceneConfiguration.CARDS_ON_BOARD_COUNT)
            {
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition + 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi, 1);
                Debug.Log("split attack right " + cardAcrossUi.card);
            }
        }

        public async Task DamageCard(Card card, Card enemyCard, CardUI cardUI, CardUI enemyCardUi)
        {
            await DamageCardDirectly(enemyCard, card, enemyCardUi, cardUI, card.damage);

            if (enemyCard.quill.IsSet)
            {
                Debug.Log($"Damaging by quill damagedCard: {card}");
                await DamageCardDirectly(card, cardUI, 1);
            }

            if (enemyCard.poisonOther.IsSet)
            {
                PoisonOther(card);
            }
        }

        private static void PoisonOther(Card card)
        {
            Debug.Log("Poison other " + card);

            if (!card.poisoned.IsSet)
            {
                card.poisoned.IsSet = true;
                card.poisoned.Value = new Poisoned();
            }

            card.poisoned.Value.level = 1;
            card.poisoned.Value.needToTick = true;
        }

        public async Task DamageCardDirectly(Card cardToDamage, Card enemyCard, CardUI cardToDamageUI,
            CardUI enemyCardUi,
            int damage)
        {
            await DamageCardDirectly(cardToDamage, cardToDamageUI, damage);
        }

        public async Task DamageCardDirectly(Card enemyCard, CardUI enemyCardUi, int damage)
        {
            if (damage <= 0)
            {
                return;
            }
            
            if (isDead(enemyCard)) return;
            
            sceneConfiguration.tableAudioSource.PlayOneShot(sceneConfiguration.cardDamageAudio);

            Debug.Log("damage dealt to card " + enemyCard);

            if (enemyCard.shield.IsSet)
            {
                Debug.Log("shield taken off " + enemyCard);

                enemyCard.shield.IsSet = false;
                enemyCard.shield.Value = null;
                return;
            }

            enemyCard.hp -= damage;
            TextPopUpSpawnerManager.Instance.StartTextPopUpTween("-" + damage, Color.red,
                enemyCardUi.transform);
            // await Task.Delay(500);
            await AnimateDamageShake(enemyCardUi);


            initializeCardSystem.ShowCardData(enemyCardUi.cardPosition, enemyCard,
                GetCardHolder(enemyCard.side));

            if (enemyCard.hp <= 0)
            {
                CardIsDead(enemyCardUi);
            }
        }

        public void CardIsDead(CardUI carenemyCardUi)
        {
            if (isDead(carenemyCardUi.card)) return;

            TextPopUpSpawnerManager.Instance.StartTextPopUpTween("Dead", Color.red,
                carenemyCardUi.transform);
            carenemyCardUi.card.IsDead = true;
            carenemyCardUi.ShowEmptyCardData();
        }

        public CardUI GetCardAcross(CardUI cardUI)
        {
            return GetCardUiListAcross(cardUI.card)[cardUI.cardPosition];
        }
        
        public CardUI GetCardAcrossAtPosition(CardUI cardUI, int position)
        {
            return GetCardUiListAcross(cardUI.card)[position];
        }



        private async Task DamageMainBoard(int damage)
        {
            sceneConfiguration.hpBalanceManagerText.transform.DOShakeScale(0.1f);
            gameContext.playerEnemyHpBalance -= damage;
        }

        /**
         *             if (card.side == Side.player)
            {
                gameContext.playerEnemyHpBalance += card.damage;
                TextPopUpSpawnerManager.Instance.StartTextPopUpTween("+" + card.damage, Color.green,
                    sceneConfiguration.hpBalanceManagerText.transform);
            }
         */
        public List<CardUI> GetCardUiListAcross(Card card)
        {
            return GetCardUiList(card.side == Side.player ? Side.enemy : Side.player);
        }

        public void RefreshCard(CardUI cardUI)
        {
            if (cardUI.card == null || cardUI.card.IsDead)
            {
                cardUI.ShowEmptyCardData();
                return;
            }

            Card card = cardUI.card;

            List<SkillObject> activeSkillObjects = new List<SkillObject>();

            if (card.poisonOther.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.poisonOther);
            }

            if (card.splitAttack.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.splitAttack);
            }

            if (card.buff.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.buff);
            }

            if (card.transformation.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.transformation);
            }

            if (card.arrowShot.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.arrowShot);
            }

            if (card.steroids.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.steroids);
            }

            if (card.summon.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.summon);
            }

            if (card.shield.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.shield);
            }

            if (card.quill.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.quill);
            }

            if (card.reduceDamage.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.reduceDamage);
            }

            if (card.gyroAttack.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.gyroAttack);
            }

            if (card.poisoned.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.deadlyPoison);
            }

            // poisoned

            cardUI.ShowCardData(cardUI.card, cardUI.cardPosition, activeSkillObjects);
        }


        public CardUI GetCardUi(Card card)
        {
            foreach (CardUI cardUI in GetCardHolder(card.side))
            {
                if (cardUI.card == card)
                {
                    return cardUI;
                }
            }

            return null;
        }

        public Card[] GetCardList(Side side)
        {
            Transform cardHolder = GetCardHolder(side);
            return cardHolder.GetComponentsInChildren<CardUI>().Select(cardUi => cardUi.card).ToArray();
        }

        public List<CardUI> GetCardUiList(Side side)
        {
            Transform cardHolder = GetCardHolder(side);
            return cardHolder.GetComponentsInChildren<CardUI>().ToList();
        }

        public Transform GetCardHolder(Side side)
        {
            return side == Side.player ? sceneConfiguration.playerCardsHolder : sceneConfiguration.enemyCardsHolder;
        }

        public bool isDead(Card card)
        {
            if (card == null || card.IsDead)
            {
                return true;
            }

            return false;
        }
    }
}