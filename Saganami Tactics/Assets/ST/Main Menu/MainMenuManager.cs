using System;
using System.Collections;
using System.Linq.Expressions;
using Michsky.UI.Shift;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ST.Main_Menu
{
    public class MainMenuManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
        [SerializeField] private ModalWindowManager messageModal;
        [SerializeField] private BlurManager blurManager;
#pragma warning restore 649

        [Header("Create game inputs")]
#pragma warning disable 649
        [SerializeField]
        private TMP_InputField playerNameField;

        [SerializeField] private TMP_InputField gameNameField;

        [SerializeField] private HorizontalSelector nbPlayersField;
        [SerializeField] private Slider maxPointsField;
#pragma warning restore 649

        [Header("Join game inputs")]
#pragma warning disable 649
        [SerializeField]
        private TMP_InputField joinPlayerNameField;

        [SerializeField] private TMP_Text joinGameNameText;
#pragma warning restore 649

        [Header("Load game")]
#pragma warning disable 649
        [SerializeField]
        private ModalWindowManager loadGameModal;
#pragma warning restore 649

        public void CreateGameFromInputs()
        {
            var gameName = gameNameField.text;
            var nbPlayers = int.Parse(nbPlayersField.itemList[nbPlayersField.index].itemTitle);
            var maxPoints = Mathf.CeilToInt(maxPointsField.value);

            if (gameName.Length <= 0 || nbPlayers <= 0 || maxPoints <= 0) return;

            CreateGame(gameName, new GameSetup() {nbPlayers = nbPlayers, maxCost = maxPoints});

            // Remember player name for next time
            PlayerPrefs.SetString("nickname", playerNameField.text);
        }

        private void CreateGame(string gameName, GameSetup setup, bool isStarted = false)
        {
            var props = new Hashtable
            {
                {GameSettings.Default.MaxPointsProp, setup.maxCost},
                {GameSettings.Default.GameStartedProp, isStarted},
                {GameSettings.Default.BluePlayerProp, setup.bluePlayer},
                {GameSettings.Default.YellowPlayerProp, setup.yellowPlayer},
                {GameSettings.Default.GreenPlayerProp, setup.greenPlayer},
                {GameSettings.Default.MagentaPlayerProp, setup.magentaPlayer}
            };

            PhotonNetwork.CreateRoom(gameName, new RoomOptions()
            {
                MaxPlayers = (byte) setup.nbPlayers,
                IsOpen = true,
                IsVisible = true,
                CustomRoomProperties = props,
                CustomRoomPropertiesForLobby = new string[]
                {
                    GameSettings.Default.MaxPointsProp,
                    GameSettings.Default.GameStartedProp,
                }
            });
        }

        public void JoinGameFromInputs()
        {
            var gameName = joinGameNameText.text;
            JoinGame(gameName);

            // Remember player name for next time
            PlayerPrefs.SetString("nickname", joinPlayerNameField.text);
        }

        private void JoinGame(string gameName)
        {
            PhotonNetwork.JoinRoom(gameName);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log($"Left room");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.LogFormat("Failed to create game: {0}", message);
            ShowMessage("Failed to create game", message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            Debug.LogFormat("Failed to join game: {0}", message);
            ShowMessage("Failed to join game", message);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Debug.LogFormat("Created game successfully: {0}", PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            StartCoroutine(WaitForPlayerHasTeam(andThen: () =>
            {
                SceneManager.LoadScene(PhotonNetwork.CurrentRoom.IsGameStarted()
                    ? GameSettings.Default.ScenePlay
                    : GameSettings.Default.SceneSetup);
            }));

            Debug.LogFormat("Joined game successfully: {0}", PhotonNetwork.CurrentRoom.Name);
        }

        private IEnumerator WaitForPlayerHasTeam(Action andThen)
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("nickname");
            PhotonNetwork.LocalPlayer.AutoAssignTeam();

            do
            {
                yield return null;
            } while (!PhotonNetwork.LocalPlayer.GetTeam().HasValue);

            Debug.Log($"Player assigned to team {PhotonNetwork.LocalPlayer.GetTeam()}");
            andThen.Invoke();
        }

        private void ShowMessage(string title, string message)
        {
            blurManager.BlurInAnim();
            messageModal.windowTitle.text = title;
            messageModal.windowDescription.text = message;
            messageModal.ModalWindowIn();
        }

        public void ShowLoadGames()
        {
            blurManager.BlurInAnim();
            loadGameModal.ModalWindowIn();
        }

        public void LoadGame(GameStateSaveSystem.SaveGameInfo gameInfo, string playerName)
        {
            // Remember player name for next time
            PlayerPrefs.SetString("nickname", playerName);

            loadGameModal.ModalWindowOut();
            blurManager.BlurOutAnim();

            var gameState = GameStateSaveSystem.Load(gameInfo.GameName);
            if (!gameState.HasValue) return;

            var gameStateContainer = FindObjectOfType<HasGameState>();

            if (gameStateContainer == null)
            {
                var gameStateGo = new GameObject {name = "_GameState"};
                DontDestroyOnLoad(gameStateGo);
                gameStateContainer = gameStateGo.AddComponent<HasGameState>();
            }

            gameStateContainer.gameState = gameState.Value;

            var setup = gameState.Value.setup;
            CreateGame(gameInfo.GameName, setup, true);
        }
    }
}