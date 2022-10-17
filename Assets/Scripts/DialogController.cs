using System.Threading.Tasks;
using Leopotam.Ecs;
using UnityEngine;

namespace Client
{
    public class DialogController : IEcsSystem
    {
        private SceneConfiguration sceneConfiguration;
        private GameContext gameContext;
        private CameraController cameraController;
        private CardAnimationSystem animationSystem;

        private CardsChoseController cardsChoseController;

        public async Task TutorialOnlyLevel(Tutorial tutorial)
        {
            cardsChoseController.ChooseCardsLevel(new[]
            {
                tutorial.tutorialCard,
                null
            });

            sceneConfiguration.shop.inventoryCardsHolder.SetActive(false);
            sceneConfiguration.shop.rollACardHolder.SetActive(false);
            sceneConfiguration.shop.nextLevelButtonObject.SetActive(false);
            sceneConfiguration.shop.sellCardHolderObject.SetActive(false);
            sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);

            DialogTextManager.Instance.ShowText(tutorial.dragFromShopSayingText);
            animationSystem.AnimateGlow(sceneConfiguration.cardsChooseHolder.transform.position);

            int money = sceneConfiguration.shop.currentMoney;
            while (sceneConfiguration.shop.currentMoney == money)
            {
                await Task.Yield();
            }

            sceneConfiguration.shop.inventoryCardsHolder.SetActive(false);
            sceneConfiguration.shop.rollACardHolder.SetActive(true);
            sceneConfiguration.shop.nextLevelButtonObject.SetActive(false);
            sceneConfiguration.shop.sellCardHolderObject.SetActive(false);
            sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);

            DialogTextManager.Instance.ShowText(tutorial.tryRollingText);
            animationSystem.AnimateGlow(sceneConfiguration.shop.rollACardHolder.transform.position);

            money = sceneConfiguration.shop.currentMoney;
            while (sceneConfiguration.shop.currentMoney == money)
            {
                await Task.Yield();
            }

            sceneConfiguration.shop.inventoryCardsHolder.SetActive(false);
            sceneConfiguration.shop.rollACardHolder.SetActive(false);
            sceneConfiguration.shop.nextLevelButtonObject.SetActive(false);
            sceneConfiguration.shop.sellCardHolderObject.SetActive(false);
            sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);

            cardsChoseController.ChooseCardsLevel(new[]
            {
                tutorial.tutorialCard,
                tutorial.tutorialCard
            });

            DialogTextManager.Instance.ShowText(tutorial.gradeYourCard);
            animationSystem.AnimateGlow(sceneConfiguration.cardsChooseHolder.transform.position);

            money = sceneConfiguration.shop.currentMoney;
            while (sceneConfiguration.shop.currentMoney
                   + tutorial.tutorialCard.card.cost * 2 != money)
            {
                await Task.Yield();
            }

            sceneConfiguration.shop.inventoryCardsHolder.SetActive(true);
            sceneConfiguration.shop.rollACardHolder.SetActive(true);
            sceneConfiguration.shop.nextLevelButtonObject.SetActive(false);
            sceneConfiguration.shop.sellCardHolderObject.SetActive(true);
            sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);

            DialogTextManager.Instance.ShowText(tutorial.youHaveInventorySayingText);
            animationSystem.AnimateGlow(sceneConfiguration.shop.inventoryCardsHolder.transform.position);

            while (CheckForDialogEnd())
            {
                await Task.Yield();
            }

            DialogTextManager.Instance.ShowText(tutorial.youCanSellCard);
            animationSystem.AnimateGlow(sceneConfiguration.shop.sellCardHolderObject.transform.position);

            while (CheckForDialogEnd())
            {
                await Task.Yield();
            }

            DialogTextManager.Instance.ShowText(tutorial.yourCardsWillRespawnText);
            animationSystem.AnimateGlow(sceneConfiguration.playerCardsHolder.transform.position);
            while (CheckForDialogEnd())
            {
                await Task.Yield();
            }

            DialogTextManager.Instance.ShowText(tutorial.nowClickNextLevelText);
            animationSystem.AnimateGlow(sceneConfiguration.shop.nextLevelButtonObject.transform.position);

            sceneConfiguration.shop.inventoryCardsHolder.SetActive(true);
            sceneConfiguration.shop.rollACardHolder.SetActive(true);
            sceneConfiguration.shop.nextLevelButtonObject.SetActive(true);
            sceneConfiguration.shop.sellCardHolderObject.SetActive(true);
            sceneConfiguration.cardsChooseHolder.gameObject.SetActive(true);
        }
        
        public async Task DialogOnlyLevel(DialogObject dialogObject)
        {
            cameraController.ShowDialogOnly();
            // cameraController.CannotLookOut = true;
            await DialogLevel(dialogObject);
            // cameraController.CannotLookOut = false;
        }

        public async Task DialogLevel(DialogObject dialogObject)
        {
            DialogTextManager.Instance.StartDialog(dialogObject);
            

            while (CheckForDialogEnd())
            {
                Debug.Log("Check for dialog end");
                await Task.Yield();
            }
            
            Debug.Log("dialog finished");
        }
        
        private bool CheckForDialogEnd()
        {
            if (sceneConfiguration.dialogText.IsActive())
            {
                return true;
            }

            return false;
        }
    }
}