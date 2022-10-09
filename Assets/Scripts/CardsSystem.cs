using System;
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

        public static event Action<CardUI> ActionCardDamaged;

        public async Task CardTurnEnemyToPlayer(CardUI cardUi, CardUI enemyCardUi)
        {
            Card card = cardUi.card;
            Card enemyCard = enemyCardUi.card;

            if (isDeadOrEmpty(card)) return;

            if (isDeadOrEmpty(enemyCard)) return;

            await CardTurnMainSkills(card, enemyCard, cardUi, enemyCardUi);
            await CardTurnMainSkills(enemyCard, card, enemyCardUi, cardUi);

            initializeCardSystem.ShowCardData(cardUi.cardPosition, card,
                sceneConfiguration.enemyCardsHolder);
            initializeCardSystem.ShowCardData(enemyCardUi.cardPosition, enemyCard,
                sceneConfiguration.playerCardsHolder);
        }

        public async Task CardsTurnAdditionalSkills(CardUI cardUI,
            CardUI enemyCardUI, Side side, int line, int turn)
        {
            foreach (CardUI crdUi in GetCardUIList(side))
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

                // and if the first row
                if (crd.horseRide.IsSet && line == 0)
                {
                    await HorseRide(cardUI);
                }

                if (crd.healOther.IsSet)
                {
                    FindAndHeal(crd);
                }

                if (crd.buff.IsSet && turn == 0)
                {
                    CardBuff(line, crd, crdUi);
                }

                if (crd.transformation.IsSet)
                {
                    await CardTransformation(crd, crdUi);
                }
            }
        }

        private async Task CardTransformation(Card crd, CardUI crdUi)
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

        private void CardBuff(int line, Card crd, CardUI crdUi)
        {
            // а давай buff это только в начале хода будет делаться
            // возьми слева и справа карту и дай им 1 хп и 1 дэмедж
            // сделай зеленым 

            List<CardUI> cardList = GetCardUIList(crd.side, line);

            if (crdUi.cardPosition - 1 >= 0)
            {
                CardUI cardToTheSideUI = cardList[crdUi.cardPosition - 1];

                if (cardToTheSideUI != null && !isDeadOrEmpty(cardToTheSideUI.card))
                {
                    cardToTheSideUI.card.hp += 1;
                    cardToTheSideUI.card.damage += 1;
                }

                Debug.Log("buff left " + cardToTheSideUI.card);
            }

            if (crdUi.cardPosition + 1 < sceneConfiguration.cardsOnBoardCount)
            {
                CardUI cardToTheSideUI = cardList[crdUi.cardPosition + 1];
                if (cardToTheSideUI != null && !isDeadOrEmpty(cardToTheSideUI.card))
                {
                    cardToTheSideUI.card.hp += 1;
                    cardToTheSideUI.card.damage += 1;
                }

                Debug.Log("buff right " + cardToTheSideUI.card);
            }
        }

        private void FindAndHeal(Card crd)
        {
            // find random guy to heal
            // add a 1 hp to him
            CardUI cardToHeal = GetCardAllUIs(crd.side)
                .Find(c =>
                    !isDeadOrEmpty(c.card)
                    && c.card.hp < c.card.cardObject.card.hp);

            if (cardToHeal != null)
            {
                cardToHeal.card.hp += 1;
                RefreshCard(cardToHeal);
            }
        }

        private async Task HorseRide(CardUI cardUI)
        {
            // attacks with 1 damage both lines in front if the first line
            CardUI acrossEnemyCardUI =
                GetCardAcross(cardUI, 0);
            if (acrossEnemyCardUI != null && !isDeadOrEmpty(acrossEnemyCardUI.card))
            {
                await DamageCardDirectly(acrossEnemyCardUI.card, acrossEnemyCardUI, 1);
            }

            CardUI acrossEnemyCardUI2 =
                GetCardAcross(cardUI, 1);
            if (acrossEnemyCardUI2 != null && !isDeadOrEmpty(acrossEnemyCardUI2.card))
            {
                await DamageCardDirectly(acrossEnemyCardUI2.card, acrossEnemyCardUI2, 1);
            }
        }

        public bool CheckDamageBoardOrNoEnemy()
        {
            List<CardUI> aliveCards = GetCardUIList(Side.player)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardUIList(Side.enemy)
                .Where(c => !isDeadOrEmpty(c.card))
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

        /**
         * так а как у нас будет выстраиваться второй третий ряд и тд врагов ?
         * - - - -
         * - - - -
         * - - - -
         * 
         * GetCardHolder(Side side, int cardLineLevel = 0)
         * var cardLineLevel = cardUiPos % sceneConfiguration.cardCountOnBoard
         * <param name="turn"></param>
         */
        public async Task IterateCardsAndDamage(int turn)
        {
            foreach (CardUI enemyCardUI in GetCardUIList(Side.enemy))
            {
                if (enemyCardUI == null || isDeadOrEmpty(enemyCardUI.card))
                {
                    continue;
                }

                CardUI playerCardUI = GetCardToAttack(enemyCardUI);
                if (playerCardUI != null && !isDeadOrEmpty(playerCardUI.card))
                {
                    await Turn(enemyCardUI, playerCardUI, turn);
                }
                else
                {
                    break;
                }
            }

            int secondLine = 1;
            foreach (CardUI enemyCardUI in GetCardUIList(Side.enemy, secondLine))
            {
                await CardsTurnAdditionalSkills(enemyCardUI, null,
                    Side.enemy, secondLine, turn);
            }

            foreach (CardUI enemyCardUI in GetCardUIList(Side.player, secondLine))
            {
                await CardsTurnAdditionalSkills(enemyCardUI, null,
                    Side.player, secondLine, turn);
            }


            await AdvancePreviousLineCards(Side.enemy); // advance and take action
            await AdvancePreviousLineCards(Side.player);
        }

        private async Task AdvancePreviousLineCards(Side side)
        {
            for (int line = 1;
                 line <
                 sceneConfiguration.maxCardsInAllSideLines / sceneConfiguration.cardsOnBoardCount;
                 line++)
            {
                List<CardUI> previousLineCards = GetCardUIList(side, line - 1);
                List<CardUI> cardsLineCards = GetCardUIList(side, line);

                Debug.Log("Card Line cards " + string.Join(",", cardsLineCards));

                if (previousLineCards.Count == 0 || cardsLineCards.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < sceneConfiguration.cardsOnBoardCount; i++)
                {
                    //  + sceneConfiguration.cardsOnBoardCount * (line - 1)
                    //  + sceneConfiguration.cardsOnBoardCount * line

                    CardUI backwardLineCard = cardsLineCards[i];
                    CardUI forwardLineCard = previousLineCards[i];

                    Debug.Log(
                        $"Check if advance card to next level backwardLineCard={backwardLineCard} forwardLineCard={forwardLineCard}");

                    if (!isDeadOrEmpty(backwardLineCard.card) && isDeadOrEmpty(forwardLineCard.card))
                    {
                        Debug.Log("Advance card to next level " + backwardLineCard.card);

                        Vector3 initialPosition = backwardLineCard.transform.position;
                        await backwardLineCard.transform
                            .DOMove(forwardLineCard.transform.position, 0.5f)
                            .AsyncWaitForCompletion();

                        backwardLineCard.MoveToStartPosition();

                        initializeCardSystem.ShowCardData(backwardLineCard.card, forwardLineCard);
                        RemoveCard(backwardLineCard);
                    }
                }
            }
        }

        private CardUI GetCardToAttack(CardUI enemyCardUI)
        {
            CardUI playerCardUI = GetCardAcross(enemyCardUI);
            if (playerCardUI == null || isDeadOrEmpty(playerCardUI.card))
            {
                for (int i = 0; i < sceneConfiguration.cardsOnBoardCount; i++)
                {
                    playerCardUI = GetCardAcrossAtPosition(enemyCardUI, i);
                    Debug.Log("Found player card " + playerCardUI);

                    if (playerCardUI != null && !isDeadOrEmpty(playerCardUI.card))
                    {
                        return playerCardUI;
                    }
                }
            }

            return playerCardUI;
        }

        public async Task Turn(CardUI enemyCardUi, CardUI playerCardUi, int turn)
        {
            // await Task.k(100);
            Debug.Log(" local pos player hold " + playerCardUi.transform.localPosition);
            await AnimateCardAttackPosition(enemyCardUi, playerCardUi);

            await CardTurnEnemyToPlayer(enemyCardUi,
                playerCardUi);

            await CardsTurnAdditionalSkills(enemyCardUi,
                playerCardUi, Side.player, 0, turn);

            await CardsTurnAdditionalSkills(enemyCardUi,
                playerCardUi, Side.enemy, 0, turn);
        }

        private static async Task AnimateCardAttackPosition(CardUI enemyCardUi, CardUI playerCardUi)
        {
            // enemyCardUi.animator.enabled = true;
            // enemyCardUi.animator.SetTrigger("Attack");
            // enemyCardUi.animator.enabled = false;

            await enemyCardUi.transform
                .DOLocalRotate(new Vector3(-40f, 0f, 0f), 0.1f)
                .AsyncWaitForCompletion();
            
            await enemyCardUi.transform
                .DOPunchPosition((playerCardUi.transform.localPosition)
                                                        - enemyCardUi.transform.localPosition
                , 0.5f, 1).AsyncWaitForCompletion();
            await enemyCardUi.transform
                .DOLocalRotate(new Vector3(0f, 0f, 0f), 0.1f)
                .AsyncWaitForCompletion();
            
            // enemyCardUi.animator.enabled = false;
        }

        private static async Task AnimateTransformShake(CardUI playerCardUi)
        {
            var localRotation = playerCardUi.transform.localRotation;
            await playerCardUi.transform.DOShakeScale(0.5f, 0.5f).AsyncWaitForCompletion();
            playerCardUi.transform.localRotation = localRotation;
        }

        // todo do separate system for animating cards
        private static async Task AnimateDamageShake(CardUI playerCardUi, CardUI oppositeCard)
        {
            var localRotation = playerCardUi.transform.localRotation;
            var tween =  playerCardUi.transform.DOShakeRotation(0.5f, 10f);
            playerCardUi.transform.localRotation = localRotation;
            var initColor = playerCardUi.cardFace.color;
            await DOTween.Sequence().Join(tween)
                .Join(playerCardUi.cardFace.DOColor(Color.red, 0.3f))
                .AsyncWaitForCompletion();
            await playerCardUi.cardFace.DOColor(initColor, 0.1f).AsyncWaitForCompletion();
        }


        public async Task<bool> EndOfInvasion()
        {
            bool levelWon;

            List<CardUI> aliveCards = GetCardUIList(Side.player)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardUIList(Side.enemy)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            if (aliveCards.Count == 0 && aliveCardsEnemy.Count != 0)
            {
                int damage = 0;

                foreach (CardUI cardUI in aliveCardsEnemy)
                {
                    damage += cardUI.card.damage;
                }

                // cameraController.ShowLeftward();
                // await cameraController.AwaitCinemachineBlending();

                gameContext.playerEnemyHpBalance -= damage;
                Debug.Log("Damaging main board with damage " + damage);
                // await Task.Delay(300);
                await AnimationDamageMainBoard();
                cameraController.ShowTable();

                levelWon = false;
            }
            else
            {
                levelWon = true;
            }

            gameContext.invasionLevel++;

            return levelWon;
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
            await DamageCardDirectly(acrossEnemyCardUI.card, acrossEnemyCardUI, 1, playerCardUI);
            Debug.Log($"Arrow shot enemy card {acrossEnemyCardUI} player card {playerCardUI}");
        }

        private async Task CardTurnMainSkills(Card card, Card enemyCard, CardUI cardUi, CardUI enemyCardUi)
        {
            if (card.damage > 0)
            {
                await DamageCard(card, enemyCard, cardUi, enemyCardUi);
            }

            if (card.splitAttack.IsSet)
            {
                await SplitAttack(cardUi, enemyCardUi);
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

        private async Task SplitAttack(CardUI cardUI, CardUI enemyCardUi)
        {
            Debug.Log("split attack " + cardUI.card);
            List<CardUI> cardsAcross = GetCardUiListAcross(cardUI.card);

            if (enemyCardUi.cardPosition - 1 >= 0)
            {
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition - 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi, 1, cardUI);
                Debug.Log("split attack left " + cardAcrossUi.card);
            }

            if (enemyCardUi.cardPosition + 1 < sceneConfiguration.cardsOnBoardCount)
            {
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition + 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi, 1, cardUI);
                Debug.Log("split attack right " + cardAcrossUi.card);
            }
        }

        public async Task DamageCard(Card card, Card enemyCard, CardUI cardUI, CardUI enemyCardUi)
        {
            await DamageCardDirectly(enemyCard,
                card, enemyCardUi, 
                cardUI, 
                card.damage);

            if (enemyCard.horseRide.IsSet)
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
            CardUI cardUI,
            int damage)
        {
            await DamageCardDirectly(cardToDamage, cardToDamageUI, damage, cardUI);
        }

        public async Task DamageCardDirectly(Card enemyCard,
            CardUI enemyCardUi, int damage, CardUI oppositeCard = null)
        {
            if (damage <= 0)
            {
                return;
            }

            if (isDeadOrEmpty(enemyCard)) return;

            ActionCardDamaged?.Invoke(enemyCardUi);

            Debug.Log("damage dealt to card " + enemyCard);

            if (enemyCard.shield.IsSet)
            {
                Debug.Log("shield taken off " + enemyCard);

                enemyCard.shield.IsSet = false;
                enemyCard.shield.Value = null;
                return;
            }

            enemyCard.hp -= damage;
            // TextPopUpSpawnerManager.Instance.StartTextPopUpTween("-" + damage, Color.red,
            //     enemyCardUi.transform);
            // await Task.Delay(500);
            await AnimateDamageShake(enemyCardUi, oppositeCard);


            initializeCardSystem.ShowCardData(enemyCardUi.cardPosition, enemyCard,
                GetCardHolder(enemyCard.side));

            if (enemyCard.hp <= 0)
            {
                CardIsDead(enemyCardUi);
            }
        }

        public void CardIsDead(CardUI carenemyCardUi)
        {
            if (isDeadOrEmpty(carenemyCardUi.card)) return;

            // TextPopUpSpawnerManager.Instance.StartTextPopUpTween("Dead", Color.red,
            //     carenemyCardUi.transform);
            carenemyCardUi.card.IsDead = true;
            carenemyCardUi.ShowEmptyCardData();
        }

        public void RemoveCard(CardUI cardToRemove)
        {
            cardToRemove.card = null;
            cardToRemove.ShowEmptyCardData();
        }

        public CardUI GetCardAcross(CardUI cardUI, int line = 0)
        {
            return GetCardUiListAcross(cardUI.card, line)[cardUI.cardPosition];
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
        public List<CardUI> GetCardUiListAcross(Card card, int line = 0)
        {
            return GetCardUIList(card.side == Side.player ? Side.enemy : Side.player, line);
        }

        public void RefreshCard(CardUI cardUI)
        {
            if (cardUI.card == null || cardUI.card.IsDead)
            {
                cardUI.ShowEmptyCardData();
                return;
            }

            Card card = cardUI.card;

            int cost = GetCost(card);
            List<SkillObject> activeSkillObjects = GetActiveSkillObjects(card);
            bool cardInInventory = shopController.GetInventoryUICards().Contains(cardUI);
            cardUI.view.SetActive(true);

            // poisoned

            cardUI.ShowCardData(cardUI.card,
                cardUI.cardPosition,
                activeSkillObjects,
                cost, cardInInventory);
        }

        public int GetCost(Card card)
        {
            // todo also add check for special trader
            if (card.itemOnly.IsSet && card.itemOnly.Value.sellHigh.IsSet)
            {
                return card.cost * 2;
            }

            return card.cost;
        }

        public List<SkillObject> GetActiveSkillObjects(Card card)
        {
            List<SkillObject> activeSkillObjects = new List<SkillObject>();

            if (card.poisonOther.IsSet) // todo just add SkillObject as a field to the skill?? 
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

            if (card.horseRide.IsSet)
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

            return activeSkillObjects;
        }

        public void AddSkillToACard(CardUI cardUI, SkillObject skillToAdd)
        {
            Card card = cardUI.card;
            if (skillToAdd == sceneConfiguration.skillsObjectsDict.poisonOther)
                // todo just add SkillObject as a field to the skill?? Or probably I will use reflection class nam
            {
                card.poisonOther.IsSet = true;
                card.poisonOther.Value = new Poison();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.splitAttack)
            {
                card.splitAttack.IsSet = true;
                card.splitAttack.Value = new SplitAttack();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.buff)
            {
                card.buff.IsSet = true;
                card.buff.Value = new Buff();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.transformation)
            {
                card.buff.IsSet = true;
                card.buff.Value = new Buff();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.arrowShot)
            {
                card.arrowShot.IsSet = true;
                card.arrowShot.Value = new ArrowShot();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.steroids)
            {
                card.steroids.IsSet = true;
                card.steroids.Value = new Steroids();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.summon)
            {
                card.summon.IsSet = true;
                card.summon.Value = new Summon();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.shield)
            {
                card.shield.IsSet = true;
                card.shield.Value = new Shield();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.quill)
            {
                card.horseRide.IsSet = true;
                card.horseRide.Value = new HorseRide();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.reduceDamage)
            {
                card.reduceDamage.IsSet = true;
                card.reduceDamage.Value = new ReduceDamage();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.gyroAttack)
            {
                card.gyroAttack.IsSet = true;
                card.gyroAttack.Value = new GyroAttack();
            }

            card.name += "+";

            RefreshCard(cardUI);

            // if (card.poisoned.IsSet)
            // {
            //     activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.deadlyPoison);
            // }
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

        public List<CardUI> GetCardUIList(Side side, int lineLevel = 0)
        {
            Debug.Log($"Get card list level={lineLevel}," +
                      $" cardsToCheck={string.Join(",", GetCardAllUIs(side))}");

            return GetCardAllUIs(side)
                .Where(cardUI => (cardUI.cardPosition / sceneConfiguration.cardsOnBoardCount) == lineLevel)
                .ToList();
        }

        public List<CardUI> GetCardAllUIs(Side side)
        {
            Transform cardHolder = GetCardHolder(side);


            return cardHolder
                .GetComponentsInChildren<CardUI>().ToList();
        }

        public Transform GetCardHolder(Side side)
        {
            return side == Side.player ? sceneConfiguration.playerCardsHolder : sceneConfiguration.enemyCardsHolder;
        }

        public static bool isDeadOrEmpty(Card card)
        {
            if (card == null || card.IsDead)
            {
                return true;
            }

            return false;
        }
    }
}