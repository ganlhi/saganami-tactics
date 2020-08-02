using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using UnityEngine;

namespace ST.Play.UI
{
    public class PlayersList : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
        [SerializeField] private Transform content;
        [SerializeField] private GameObject playerButtonPrefab;
#pragma warning restore 649

        private void Start()
        {
            UpdateUi();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            UpdateUi();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            UpdateUi();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            UpdateUi();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            UpdateUi();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (changedProps.ContainsKey(GameSettings.ColorIndexProp) ||
                changedProps.ContainsKey(GameSettings.ReadyProp)) UpdateUi();
        }

        private void UpdateUi()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                var btn = Instantiate(playerButtonPrefab).GetComponent<PlayersListButton>();
                btn.player = player;
                btn.transform.SetParent(content);
                btn.transform.localScale = Vector3.one;
            }
        }
    }
}