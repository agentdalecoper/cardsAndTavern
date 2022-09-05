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