using System;
using Leopotam.Ecs;
using TMPro;
using UnityEngine;

namespace Client
{
    public class DialogTextManager : MonoBehaviour, IEcsSystem
    {
        public AudioSource audioSource;

        private static DialogTextManager instance;
        public static DialogTextManager Instance => instance;

        private DialogObject currentDialogObject;
        private int dialogPos = 0;
        private TextMeshProUGUI dialogTextUi;

        private SceneConfiguration sceneConfiguration;
        private GameContext gameContext;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (currentDialogObject != null)
                {
                    ShowNextDialog();
                }
                else if (dialogTextUi.gameObject.activeSelf)
                {
                    dialogTextUi.gameObject.SetActive(false);
                }
            }
        }

        public void StartDialog(DialogObject dialogObject)
        {
            if (dialogTextUi == null)
            {
                dialogTextUi = sceneConfiguration.dialogText;
            }

            currentDialogObject = dialogObject;
            dialogPos = 0;

            ShowNext();
        }

        private void ShowNextDialog()
        {
            audioSource.Play();

            dialogPos++;
            if (dialogPos >= currentDialogObject.dialogTexts.Length)
            {
                dialogTextUi.gameObject.SetActive(false);
                currentDialogObject = null;
                return;
            }

            ShowNext();
        }

        private void ShowNext()
        {
            dialogTextUi.gameObject.SetActive(true);
            dialogTextUi.text = currentDialogObject.dialogTexts[dialogPos];
        }

        public void ShowText(string text)
        {
            if (dialogTextUi == null)
            {
                dialogTextUi = sceneConfiguration.dialogText;
            }

            dialogTextUi.gameObject.SetActive(true);
            dialogTextUi.text = text;
        }
    }
}