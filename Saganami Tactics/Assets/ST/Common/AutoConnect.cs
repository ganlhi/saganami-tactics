using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ST.Common
{
    public class AutoConnect : MonoBehaviourPunCallbacks
    {
        public GameObject manager; 
        
        private void Awake()
        {
            if (PhotonNetwork.IsConnected || !Application.isEditor)
            {
                Destroy(gameObject);
            }

            Debug.Log("Auto connect");
            manager.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.CreateRoom("AutoConnect_Room", new RoomOptions()
            {
                IsOpen = false,
                IsVisible = false,
                MaxPlayers = 1,
            });
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log("Create room failed: " + message);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            PhotonNetwork.LocalPlayer.CycleColorIndex();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (changedProps.ContainsKey(GameSettings.ColorIndexProp))
            {
                manager.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}