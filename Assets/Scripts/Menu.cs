using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Nudi.Fpsproject
{
    public class Menu : MonoBehaviourPunCallbacks
    {
        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        // once connected to photon
        public override void OnConnectedToMaster()
        {
            Join();

            base.OnConnectedToMaster();

        }

        public void Connect()
        {
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();

        }

        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();

        }

        public override void OnJoinedRoom()
        {
            StartGame();

            base.OnJoinedRoom();

        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();

            base.OnJoinRandomFailed(returnCode, message);

        }

        public void Create()
        {
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel(1);
            }

        }
    }
}
