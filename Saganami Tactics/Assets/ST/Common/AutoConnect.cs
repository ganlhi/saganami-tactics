using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ST.Scriptable;
using UnityEngine;

namespace ST.Common
{
    public class AutoConnect : MonoBehaviourPunCallbacks
    {
        [Range(1, 4)] public int expectedPlayers = 1;

        public bool gameStarted;
        public int maxPoints = 100;
        public GameObject manager;
        public List<CanvasGroup> groups;

        private void Awake()
        {
            if (PhotonNetwork.IsConnected)
            {
                EnableCanvasGroups(true);
                Destroy(gameObject);
                return;
            }

            Debug.Log("Auto connect");
            manager.SetActive(false);
            EnableCanvasGroups(false);
            PhotonNetwork.ConnectUsingSettings();
        }

        private void EnableCanvasGroups(bool enable)
        {
            groups.ForEach(cg => cg.alpha = enable ? 1 : 0);
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            var props = new Hashtable
            {
                {GameSettings.Default.MaxPointsProp, maxPoints},
                {GameSettings.Default.GameStartedProp, false} // will be true for loaded game
            };

            PhotonNetwork.JoinOrCreateRoom("AutoConnect_Room", new RoomOptions()
            {
                IsOpen = true,
//                IsVisible = false,
                MaxPlayers = (byte) expectedPlayers,
//                EmptyRoomTtl = 120000,
//                PlayerTtl = 120000,
                CustomRoomProperties = props
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

//            var n = PhotonNetwork.CurrentRoom.PlayerCount;
//            PhotonNetwork.LocalPlayer.NickName = "Player "+n;
//            PhotonNetwork.LocalPlayer.SetColorIndex(n);
            PhotonNetwork.LocalPlayer.AssignFirstAvailableColorIndex();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer.IsLocal && changedProps.ContainsKey(GameSettings.Default.ColorIndexProp))
            {
                manager.SetActive(true);
                EnableCanvasGroups(true);
                Destroy(gameObject);
            }
        }
    }
}