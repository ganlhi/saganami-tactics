using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ST.Scriptable;
using UnityEngine;

namespace ST.Common
{
    public class AutoConnect : MonoBehaviourPunCallbacks
    {
        [Range(1, 4)]
        public int expectedPlayers = 1; 
        public GameObject manager; 
        
        private void Awake()
        {
            if (PhotonNetwork.IsConnected)
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
            PhotonNetwork.JoinOrCreateRoom("AutoConnect_Room", new RoomOptions()
            {
                IsOpen = true,
                IsVisible = false,
                MaxPlayers = (byte)expectedPlayers,
            }, TypedLobby.Default);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log("Create room failed: " + message);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            var n = PhotonNetwork.CurrentRoom.PlayerCount;
            PhotonNetwork.LocalPlayer.NickName = "Player "+n;
            PhotonNetwork.LocalPlayer.SetColorIndex(n);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer.IsLocal && changedProps.ContainsKey(GameSettings.Default.ColorIndexProp))
            {
                manager.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}