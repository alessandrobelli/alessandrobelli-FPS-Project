using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;

namespace Com.Nudi.Fpsproject
{
    public class MainMenu : MonoBehaviour
    {
        public MultiplayerController MultiplayerController;

        public void JoinMatch()
        {
            MultiplayerController.Join();
            // if not multiplayer
            // unity engine scene management + scenemanager loadscene
        }

        public void CreateMatch()
        {
            MultiplayerController.Create();
        }

        public void QuitGame()
        {
            Application.Quit();
        }


    }
}