using System.Collections;
using System.Collections.Generic;
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
        [SerializeField]
        private Transform listContent;

        [SerializeField]
        private GameObject listEntryPrefab;
#pragma warning restore 0649
        
//        private Animator _mWindowAnimator;
//        private bool _isOn;
//
//        private void Start()
//        {
//            _mWindowAnimator = gameObject.GetComponent<Animator>();
//        }
//
//        public void ManageServerList()
//        {
//            if (!_isOn)
//            {
//                _mWindowAnimator.CrossFade("List Minimize", 0.1f);
//                _isOn = true;
//            }
//            else
//            {
//                _mWindowAnimator.CrossFade("List Expand", 0.1f);
//                _isOn = false;
//            }
//        }
//
//        public void ExpandServerList()
//        {
//            if (!_isOn) return;
//            _mWindowAnimator.CrossFade("List Expand", 0.1f);
//            _isOn = false;
//        }
//
//        public void MinimizeServerList()
//        {
//            if (_isOn) return;
//            _mWindowAnimator.CrossFade("List Minimize", 0.1f);
//            _isOn = true;
//        }

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
//                child.GetComponent<RoomsListEntry>().JoinEvent.RemoveAllListeners();
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
                Debug.Log($"Room: {ri.Name}");
                
                var entry = Instantiate(listEntryPrefab, listContent);
                
                entry.transform.Find("Content/Title").GetComponent<TextMeshProUGUI>().text = ri.Name;
                entry.transform.Find("Content/Players").GetComponent<TextMeshProUGUI>().text = $"{ri.PlayerCount}/{ri.MaxPlayers}";
                entry.transform.Find("Content/MaxPoints").GetComponent<TextMeshProUGUI>().text = ri.GetMaxPoints().ToString();

                var entryBtn = entry.GetComponent<Button>();
                
                entryBtn.interactable = ri.IsOpen && ri.PlayerCount < ri.MaxPlayers;
                entryBtn.onClick.AddListener(() => Join(ri.Name));
            }
        }

        public void Join(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
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