using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Nudi.Fpsproject
{
    public class Menu : MonoBehaviour
    {
        private void OnEnable() 
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        public void Connect()
        {

        }

        public void Join()
        {

        }

        public void StartGame()
        {

        }
    }
}
