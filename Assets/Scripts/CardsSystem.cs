using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
        private CardAnimationSystem cardAnimationSystem;

        public static event Action<CardUI> ActionCardDamaged;

        public async UniTask CardTurnEnemyToPlayer(CardUI cardUi, CardUI enemyCardUi)
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

        public async UniTask CardsPreTurnSkills(Side side)
        {
            foreach (CardUI crdUi in GetCardAllUIs(side))
            {
                if (crdUi == null || isDeadOrEmpty(crdUi.card))
                {
                    continue;
                }
                
                Card crd = crdUi.card;
                int line = crdUi.cardPosition / sceneConfiguration.cardsOnBoardCount;
                
                if (crd.buff != null && crd.buff.IsSet) // todo move it into the inital turn
                {
                    if(await CardBuff(line, crd, crdUi))
                    {
                        await cardAnimationSystem.AnimateSkillUsed(crdUi,
                            sceneConfiguration.skillsObjectsDict.buff.sprite);
                    }
                }
            }
        }

        public async UniTask CardsTurnAdditionalSkills(Side side, int turn)
        {
            foreach (CardUI crdUi in GetCardAllUIs(side))
            {
                Card crd = crdUi.card;
                int line = crdUi.cardPosition / sceneConfiguration.cardsOnBoardCount;

                if (crdUi.card == null || crdUi.card.IsDead)
                {
                    continue;
                }

                if (crdUi.card.arrowShot.IsSet) // && crdUi.card != cardUI.card
                {
                    await ArrowShot(crdUi);
                }

                if (crd.poisoned.IsSet)
                {
                    await CheckDamageWithPoison(crdUi, crd);
                    // todo animation
                }

                // and if the first row
                if (crd.horseRide.IsSet && line == 0)
                {
                    await HorseRide(crdUi);
                    await cardAnimationSystem.AnimateSkillUsed(crdUi,
                        sceneConfiguration.skillsObjectsDict.horseRide.sprite);
                }

                if (crd.healOther.IsSet)
                {
                    await Heal(crd, crdUi);
                }
                
                if (crd.transformation.IsSet)
                {
                    await CardTransformation(crd, crdUi);
                }

                if (crd.summon.IsSet)
                {
                    Summon(crdUi);
                }
            }
        }

        private async UniTask Heal(Card crd, CardUI crdUi)
        {
            CardUI toHealCard = FindAndHeal(crd);

            if (toHealCard != null)
            {
                cardAnimationSystem.AnimateSkillUsed(crdUi,
                    sceneConfiguration.skillsObjectsDict.healOther.sprite);
                await cardAnimationSystem
                    .AnimateChangeOfStat(toHealCard.hpText, Color.green, true);
            }
        }

        private async UniTask Summon(CardUI crdUi)
        {
            Card crd = crdUi.card;
            Summon summon = crd.summon.Value;
            if (summon.turnsToSummon > 0)
            {
                summon.turnsToSummon--;
            }
            else
            {
                CardUI freeCellUI =
                    GetCardAllUIs(crd.side).Find(ui => isDeadOrEmpty(ui.card));
                if (freeCellUI != null)
                {
                    await cardAnimationSystem.AnimateSkillUsed(crdUi,
                        sceneConfiguration.skillsObjectsDict.summon.sprite);
                    await initializeCardSystem.CreateAndShowCard(freeCellUI, crd.side, summon.cardToSummon);
                }

                summon.turnsToSummon = crd.cardObject.card.summon.Value.turnsToSummon;
            }
        }

        private async UniTask CardTransformation(Card crd, CardUI crdUi)
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
                await cardAnimationSystem.AnimateSkillUsed(crdUi,
                    sceneConfiguration.skillsObjectsDict.transformation.sprite); // crdUi.cardFace.color = Color.red;
                // initializeCardSystem.SetCardAtPosition(crdUi.cardPosition, gameContext, crd);
            }
        }

        private async UniTask<bool> CardBuff(int line, Card crd, CardUI crdUi)
        {
            // а давай buff это только в начале хода будет делаться
            // возьми слева и справа карту и дай им 1 хп и 1 дэмедж
            // сделай зеленым 

            List<CardUI> cardList = GetCardAllUIs(crd.side);

            bool used = false;

            if (crdUi.cardPosition - 1 >= 0)
            {
                CardUI cardToTheSideUI = cardList[crdUi.cardPosition - 1];

                if (cardToTheSideUI != null && !isDeadOrEmpty(cardToTheSideUI.card))
                {
                    cardToTheSideUI.card.hp += crd.buff.Value.buff;
                    cardToTheSideUI.card.damage += crd.buff.Value.buff;
                    cardAnimationSystem.AnimateChangeOfStat(cardToTheSideUI.hpText, Color.blue);
                    cardAnimationSystem.AnimateChangeOfStat(cardToTheSideUI.damageText, Color.blue);
                    RefreshCard(cardToTheSideUI);
                    used = true;
                }

                Debug.Log($"buff left ind={crdUi.cardPosition - 1} " + cardToTheSideUI.card);
            }

            if (crdUi.cardPosition + 1 < sceneConfiguration.maxCardsInAllSideLines)
            {
                CardUI cardToTheSideUI = cardList[crdUi.cardPosition + 1];
                if (cardToTheSideUI != null && !isDeadOrEmpty(cardToTheSideUI.card))
                {
                    cardToTheSideUI.card.hp += crd.buff.Value.buff;
                    cardToTheSideUI.card.damage += crd.buff.Value.buff;
                    cardAnimationSystem.AnimateChangeOfStat(cardToTheSideUI.hpText, Color.blue);
                    cardAnimationSystem.AnimateChangeOfStat(cardToTheSideUI.damageText, Color.blue);
                    RefreshCard(cardToTheSideUI);
                    used = true;
                }
                
                Debug.Log($"buff right ind={crdUi.cardPosition + 1} " + cardToTheSideUI.card);
            }

            return used;
        }

        private CardUI FindAndHeal(Card crd)
        {
            // find random guy to heal
            // add a 1 hp to him
            CardUI cardToHeal = GetCardAllUIs(crd.side)
                .Find(c =>
                    !isDeadOrEmpty(c.card)
                    && c.card.hp < c.card.cardObject.card.hp);

            if (cardToHeal != null)
            {
                cardToHeal.card.hp += crd.healOther.Value;
                RefreshCard(cardToHeal);
            }

            return cardToHeal;
        }

        private async UniTask HorseRide(CardUI cardUI)
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
            List<CardUI> aliveCards = GetCardAllUIs(Side.player)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardAllUIs(Side.enemy)
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
        public async UniTask IterateCardsAndDamage(int turn)
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

            if (CheckDamageBoardOrNoEnemy())
            {
                return;
            }

            await CardsTurnAdditionalSkills(Side.enemy, turn);
            await CardsTurnAdditionalSkills(Side.player, turn);

            await AdvancePreviousLineCards(Side.enemy); // advance and take action
            await AdvancePreviousLineCards(Side.player);
        }

        private async UniTask AdvancePreviousLineCards(Side side)
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

        public async UniTask Turn(CardUI enemyCardUi, CardUI playerCardUi, int turn)
        {
            // await UniTask.k(100);
            Debug.Log(" local pos player hold " + playerCardUi.transform.localPosition);
            await cardAnimationSystem.AnimateCardAttackPosition(enemyCardUi, playerCardUi);

            await CardTurnEnemyToPlayer(enemyCardUi,
                playerCardUi);

            // await CardsTurnAdditionalSkills(enemyCardUi,
            //     playerCardUi, Side.player, 0, turn);
            //
            // await CardsTurnAdditionalSkills(enemyCardUi,
            //     playerCardUi, Side.enemy, 0, turn);
        }

        private static async UniTask AnimateTransformShake(CardUI playerCardUi)
        {
            var localRotation = playerCardUi.transform.localRotation;
            await playerCardUi.transform.DOShakeScale(0.5f, 0.5f).AsyncWaitForCompletion();
            playerCardUi.transform.localRotation = localRotation;
        }


        public async UniTask<bool> EndOfInvasion()
        {
            bool levelWon;

            List<CardUI> aliveCards = GetCardAllUIs(Side.player)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            List<CardUI> aliveCardsEnemy = GetCardAllUIs(Side.enemy)
                .Where(c => !isDeadOrEmpty(c.card))
                .ToList();

            if (aliveCards.Count == 0 && aliveCardsEnemy.Count != 0)
            {
                int damage = 0;

                foreach (CardUI cardUI in aliveCardsEnemy)
                {
                    damage += 1;
                }

                // cameraController.ShowLeftward();
                // await cameraController.AwaitCinemachineBlending();

                gameContext.playerEnemyHpBalance -= damage;
                Debug.Log("Damaging main board with damage " + damage);
                // await UniTask.Delay(300);
                await AnimationDamageMainBoard();
                cameraController.ShowTable();

                if (gameContext.playerEnemyHpBalance <= 0)
                {
                    sceneConfiguration.gameIsLostCanvas.gameObject.SetActive(true);
                }

                levelWon = false;
            }
            else
            {
                levelWon = true;
            }

            gameContext.invasionLevel++;

            return levelWon;
        }

        private async UniTask AnimationDamageMainBoard()
        {
            sceneConfiguration.hpBalanceManagerText.text = gameContext.playerEnemyHpBalance.ToString();
            await cardAnimationSystem.AnimateChangeOfStat(sceneConfiguration.hpBalanceManagerText,
                Color.red);
        }

        private async UniTask CheckDamageWithPoison(CardUI cardUI, Card card)
        {
            Debug.Log("check damage with poison");

            Poisoned poisoned = card.poisoned.Value;

            // if (poisoned.needToTick)
            // {
            //     poisoned.needToTick = false;
            // }
            // else
            // {
            await DamageCardDirectly(card, cardUI, 1, null, 
                new Optional<Color> {Value = Color.green});
            // card.poisoned.Value = null;
            // card.poisoned.IsSet = false;
            // }

            RefreshCard(cardUI);
        }

        private async UniTask<bool> ArrowShot(CardUI playerCardUI)
        {
            CardUI acrossEnemyCardUI =
                GetCardAcrossFromAll(playerCardUI);

            if (acrossEnemyCardUI != null && !isDeadOrEmpty(acrossEnemyCardUI.card))
            {
                await cardAnimationSystem.AnimateSkillUsed(playerCardUI,
                    sceneConfiguration.skillsObjectsDict.arrowShot.sprite);
                await DamageCardDirectly(acrossEnemyCardUI.card, acrossEnemyCardUI,
                    playerCardUI.card.arrowShot.Value.damage, playerCardUI);
                Debug.Log($"Arrow shot enemy card {acrossEnemyCardUI} player card {playerCardUI}");

                return true;
            }
            else
            {
                return false;
            }
            
        }

        private async UniTask CardTurnMainSkills(Card card, Card enemyCard, CardUI cardUi, CardUI enemyCardUi)
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
                await cardAnimationSystem.AnimateSkillUsed(cardUi,
                    sceneConfiguration.skillsObjectsDict.gyroAttack.sprite);
            }
        }

        private async UniTask GyroAttack(Card card)
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

        private async UniTask SplitAttack(CardUI cardUI, CardUI enemyCardUi)
        {
            bool used = false;

            Debug.Log("split attack " + cardUI.card);
            List<CardUI> cardsAcross = GetCardUiListAcross(cardUI.card);

            if (enemyCardUi.cardPosition - 1 >= 0)
            {
                used = true;
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition - 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi, 
                    enemyCardUi.card.splitAttack.Value.damage, cardUI);
                Debug.Log("split attack left " + cardAcrossUi.card);
            }

            if (enemyCardUi.cardPosition + 1 < sceneConfiguration.cardsOnBoardCount)
            {
                used = true;
                CardUI cardAcrossUi = cardsAcross[enemyCardUi.cardPosition + 1];
                await DamageCardDirectly(cardAcrossUi.card, cardAcrossUi,
                    enemyCardUi.card.splitAttack.Value.damage, cardUI);
                Debug.Log("split attack right " + cardAcrossUi.card);
            }

            if (used)
            {
                await cardAnimationSystem.AnimateSkillUsed(cardUI,
                    sceneConfiguration.skillsObjectsDict.splitAttack.sprite);
            }
        }

        public async UniTask DamageCard(Card card, Card enemyCard, CardUI cardUI, CardUI enemyCardUi)
        {
            await DamageCardDirectly(enemyCard,
                card, enemyCardUi, 
                cardUI, 
                card.damage);

            // if (enemyCard.horseRide.IsSet)
            // {
            //     Debug.Log($"Damaging by quill damagedCard: {card}");
            //     cardAnimationSystem.AnimateSkillUsed(cardUI,
            //         sceneConfiguration.skillsObjectsDict.quill.sprite);
            //     await DamageCardDirectly(card, cardUI, 1);
            // }

            if (enemyCard.poisonOther.IsSet)
            {
                await cardAnimationSystem.AnimateSkillUsed(cardUI,
                    sceneConfiguration.skillsObjectsDict.poisonOther.sprite);
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

        public async UniTask DamageCardDirectly(Card cardToDamage, Card enemyCard, CardUI cardToDamageUI,
            CardUI cardUI,
            int damage)
        {
            await DamageCardDirectly(cardToDamage, cardToDamageUI, damage, cardUI);
        }

        public async UniTask DamageCardDirectly(Card enemyCard,
            CardUI enemyCardUi, int damage,
            CardUI oppositeCard = null, Optional<Color> color = null)
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
            // await UniTask.Delay(500);
            await cardAnimationSystem.AnimateDamageShake(enemyCardUi, oppositeCard, color);

            initializeCardSystem.ShowCardData(enemyCardUi.cardPosition, enemyCard,
                GetCardHolder(enemyCard.side));

            if (enemyCard.hp <= 0)
            {
                await CardIsDead(enemyCardUi);
            }
        }

        public async UniTask CardIsDead(CardUI carenemyCardUi)
        {
            if (isDeadOrEmpty(carenemyCardUi.card)) return; // already dead

            // TextPopUpSpawnerManager.Instance.StartTextPopUpTween("Dead", Color.red,
            //     carenemyCardUi.transform);
            carenemyCardUi.card.IsDead = true;
            await cardAnimationSystem.AnimateCardIsDead(carenemyCardUi);
            carenemyCardUi.ShowEmptyCardData();
        }

        public void RemoveCard(CardUI cardToRemove)
        {
            cardToRemove.card = null;
            cardToRemove.ShowEmptyCardData();
        }

        public CardUI GetCardAcrossFromAll(CardUI cardUI)
        {
            return GetCardUIList(cardUI.card.side == Side.player
                ? Side.enemy : Side.player)[cardUI.cardPosition % sceneConfiguration.cardsOnBoardCount];
        }

        public CardUI GetCardAcross(CardUI cardUI, int line = 0)
        {
            return GetCardUiListAcross(cardUI.card, line)[cardUI.cardPosition];
        }

        public CardUI GetCardAcrossAtPosition(CardUI cardUI, int position)
        {
            return GetCardUiListAcross(cardUI.card)[position];
        }

        private async UniTask DamageMainBoard(int damage)
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
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.horseRide);
            }

            if (card.healOther.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.healOther);
            }

            if (card.gyroAttack.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.gyroAttack);
            }

            if (card.poisoned.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.deadlyPoison);
            }

            if (card.itemOnly.IsSet && card.itemOnly.Value.income.IsSet)
            {
                activeSkillObjects.Add(sceneConfiguration.skillsObjectsDict.income);
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
                card.buff.Value = new Buff() { buff = 2};
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.transformation)
            {
                card.buff.IsSet = true;
                card.buff.Value = new Buff();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.arrowShot)
            {
                card.arrowShot.IsSet = true;
                card.arrowShot.Value = new ArrowShot() {damage = 2};
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

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.healOther)
            {
                card.horseRide.IsSet = true;
                card.horseRide.Value = new HorseRide();
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.horseRide)
            {
                card.healOther.IsSet = true;
            }

            if (skillToAdd == sceneConfiguration.skillsObjectsDict.gyroAttack)
            {
                card.gyroAttack.IsSet = true;
                card.gyroAttack.Value = new GyroAttack();
            }

            // card.name += "+";

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
            return GetCardAllUIs(side).Select(cardUi => cardUi.card).ToArray();
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
            // Transform cardHolder = GetCardHolder(side);
            //
            //
            // return cardHolder
            //     .GetComponentsInChildren<CardUI>().ToList();

            if (side == Side.player)
            {
                return gameContext.playerCardUIs.ToList();
            }
            else
            {
                return gameContext.enemyCardUIs.ToList();
            }
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