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
        
        public string bluePlayer;
        public string yellowPlayer;
        public string greenPlayer;
        public string magentaPlayer;
        
        public string playerName;
        
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
            PhotonNetwork.AutomaticallySyncScene = true;
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
                {GameSettings.Default.GameStartedProp, false},
                {GameSettings.Default.BluePlayerProp, bluePlayer}, 
                {GameSettings.Default.YellowPlayerProp, yellowPlayer}, 
                {GameSettings.Default.GreenPlayerProp, greenPlayer}, 
                {GameSettings.Default.MagentaPlayerProp, magentaPlayer}, 
            };

            PhotonNetwork.JoinOrCreateRoom("AutoConnect_Room", new RoomOptions()
            {
                IsOpen = true,
//                IsVisible = false,
                MaxPlayers = (byte) expectedPlayers,
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

            var n = PhotonNetwork.CurrentRoom.PlayerCount;
            PhotonNetwork.LocalPlayer.NickName = playerName ?? $"Player {n}";
            PhotonNetwork.LocalPlayer.AutoAssignTeam();
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