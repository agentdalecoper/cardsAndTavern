using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client
{
    public class CameraController : IEcsSystem
    {
        // public List<CameraScene> cameraScenes;
        public static event Action onUiSwitched;

        private SceneConfiguration sceneConfiguration;

        public bool CannotLookOut;
        public bool CannotLookOutWhileBlending;

        public void CheckInput()
        {
            if (CannotLookOut)
            {
                return;
            }

            if (CannotLookOutWhileBlending && sceneConfiguration.cinemachineBrain.IsBlending)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                ShowRightward();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                ShowLeftward();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ShowTable();
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                ShowFront();
            }
        }

        public async Task AwaitCinemachineBlending()
        {
            while (sceneConfiguration.cinemachineBrain.IsBlending)
            {
                await Task.Yield();
            }
            // await Task.Delay(200);
        }

        public async void ShowFront()
        {
            sceneConfiguration.cinemachineBrain.m_DefaultBlend.m_Time = sceneConfiguration.cinemaBlendingLongTime;

            sceneConfiguration.leftwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.rightwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.tableVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.forwardVirtualCamera.gameObject.SetActive(true);

            if (sceneConfiguration.monster.isActiveAndEnabled)
            {
                // await Task.Delay(1000);
                sceneConfiguration.audioSource.gameObject.SetActive(true);
                sceneConfiguration.mystAudioSource.Stop();
                sceneConfiguration.planeGrid.MakeScary();
                // set canvas shader to scary
            }
        }

        public void ShowTable()
        {
            sceneConfiguration.cinemachineBrain.m_DefaultBlend.m_Time = sceneConfiguration.cinemaBlendingTime;

            sceneConfiguration.leftwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.rightwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.tableVirtualCamera.gameObject.SetActive(true);
            sceneConfiguration.forwardVirtualCamera.gameObject.SetActive(false);
        }

        public void ShowLeftward()
        {
            sceneConfiguration.cinemachineBrain.m_DefaultBlend.m_Time = sceneConfiguration.cinemaBlendingTime;

            sceneConfiguration.leftwardVirtualCamera.gameObject.SetActive(true);
            sceneConfiguration.rightwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.tableVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.forwardVirtualCamera.gameObject.SetActive(false);
        }

        private void ShowRightward()
        {
            sceneConfiguration.cinemachineBrain.m_DefaultBlend.m_Time = sceneConfiguration.cinemaBlendingTime;

            sceneConfiguration.leftwardVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.rightwardVirtualCamera.gameObject.SetActive(true);
            sceneConfiguration.tableVirtualCamera.gameObject.SetActive(false);
            sceneConfiguration.forwardVirtualCamera.gameObject.SetActive(false);
        }

        public void ShowInvasionAndPlayerCards()
        {
            sceneConfiguration.playerCardsHolder.parent.gameObject.SetActive(true);
            sceneConfiguration.playerCardsHolder.gameObject.SetActive(true);
            sceneConfiguration.enemyCardsHolder.gameObject.SetActive(true);
            sceneConfiguration.cardsChooseHolder.parent.gameObject.SetActive(false);
        }

        public void ShowPlayerOnlyCards()
        {
            sceneConfiguration.playerCardsHolder.parent.gameObject.SetActive(true);
            sceneConfiguration.playerCardsHolder.gameObject.SetActive(true);
            sceneConfiguration.cardsChooseHolder.parent.gameObject.SetActive(false);
            sceneConfiguration.enemyCardsHolder.gameObject.SetActive(false);
        }

        public void ShowDialogOnly()
        {
            // sceneConfiguration.playerCardsHolder.parent.gameObject.SetActive(false);
            // sceneConfiguration.cardsChooseHolder.parent.gameObject.SetActive(false);
            sceneConfiguration.dialogText.gameObject.SetActive(true);
            ShowFront();
        }

        public void ShowChosenOverlayCard()
        {
            sceneConfiguration.choosenCardCameraOverlay.gameObject.SetActive(true);
        }

        public async void ShowGameLost(string str)
        {
            sceneConfiguration.endGameCanvas.gameObject.SetActive(true);
            sceneConfiguration.endText.gameObject.SetActive(true);
            sceneConfiguration.endText.text = $"End game. {str}";

            await FadeOut();

            sceneConfiguration.mainGameObject.SetActive(false);
        }

        public async void ShowGameWon()
        {
            sceneConfiguration.endGameCanvas.gameObject.SetActive(true);
            sceneConfiguration.endText.gameObject.SetActive(true);
            sceneConfiguration.endText.text = "You made proper sacrifices. Thanks for playing!";

            await FadeOut();

            sceneConfiguration.mainGameObject.SetActive(false);
        }

        public async Task FadeIn()
        {
            Color col = sceneConfiguration.endGameImage.color;
            while (sceneConfiguration.endGameImage.color.a > 0)
            {
                sceneConfiguration.endGameImage.color =
                    new Color(col.r, col.g, col.b, sceneConfiguration.endGameImage.color.a - 0.01f);
                await Task.Yield();
                // await Task.Delay(10);
            }
        }

        public async Task FadeOut()
        {
            Color col = sceneConfiguration.endGameImage.color;
            while (sceneConfiguration.endGameImage.color.a < 1)
            {
                sceneConfiguration.endGameImage.color =
                    new Color(col.r, col.g, col.b, sceneConfiguration.endGameImage.color.a + 0.01f);
                // await Task.Delay(10);
            }
        }
    }
}