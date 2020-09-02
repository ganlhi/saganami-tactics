using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Shift;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Main_Menu
{
    public class ServerBrowserManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 0649
        [SerializeField] private Transform listContent;

        [SerializeField] private GameObject listEntryPrefab;

        [SerializeField] private BlurManager blurManager;

        [SerializeField] private ModalWindowManager joinGameModal;
#pragma warning restore 0649

        public void ListRooms()
        {
            Debug.Log("ListRooms");
            if (!PhotonNetwork.InLobby)
            {
                Debug.Log("Joining lobby");
                PhotonNetwork.JoinLobby();
            }
            else
            {
                Debug.Log("Already in lobby");
            }

            // Empty current list
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("Joined lobby");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.LogFormat("Rooms number: {0}", roomList.Count);

            // Fill with new list items
            foreach (var ri in roomList)
            {
                var entry = Instantiate(listEntryPrefab, listContent);

                entry.transform.Find("Content/Title").GetComponent<TextMeshProUGUI>().text = ri.Name;
                entry.transform.Find("Content/Players").GetComponent<TextMeshProUGUI>().text =
                    $"{ri.PlayerCount}/{ri.MaxPlayers}";
                entry.transform.Find("Content/MaxPoints").GetComponent<TextMeshProUGUI>().text =
                    ri.GetMaxPoints().ToString();

                var entryBtn = entry.GetComponent<Button>();

                entryBtn.interactable = ri.IsOpen && ri.PlayerCount < ri.MaxPlayers;
                entryBtn.onClick.AddListener(() => ShowJoinModal(ri));
            }
        }

        private void ShowJoinModal(RoomInfo roomInfo)
        {
            blurManager.BlurInAnim();
            joinGameModal.ModalWindowIn();

            joinGameModal.transform.Find("Content/Content/Game Name/Text")
                .GetComponent<TMP_Text>().text = roomInfo.Name;

            joinGameModal.transform.Find("Content/Content/Players/Text")
                .GetComponent<TMP_Text>().text = roomInfo.MaxPlayers.ToString();

            joinGameModal.transform.Find("Content/Content/Max Points/Text")
                .GetComponent<TMP_Text>().text = roomInfo.GetMaxPoints().ToString();

            joinGameModal.transform.Find("Content/Content/Is Started/Text")
                .GetComponent<TMP_Text>().text = roomInfo.IsGameStarted() ? "Yes" : "No";
        }

        public void Refresh()
        {
            PhotonNetwork.LeaveLobby();
            StartCoroutine(RejoinLobby());
        }

        private IEnumerator RejoinLobby()
        {
            do
            {
                yield return null;
            } while (PhotonNetwork.InLobby);

            ListRooms();
        }
    }
}