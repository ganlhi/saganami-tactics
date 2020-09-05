using System;
using System.IO;
using System.Linq;
using Photon.Pun;
using ST.Common;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ST.EndGame
{
    public class EndGameManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
        [SerializeField] private Transform columns;
        [SerializeField] private GameObject teamScoresPrefab;
        [SerializeField] private GameObject scoreLinePrefab;
#pragma warning restore 649

        private void Start()
        {
            foreach (Transform child in columns)
            {
                Destroy(child.gameObject);
            }

            var gameName = PhotonNetwork.CurrentRoom.Name;
            var players = PhotonNetwork.CurrentRoom.Players.Values;
            
            // Delete saved file if any
            GameStateSaveSystem.DeleteGame(gameName);

            var gameObjectHolder = FindObjectOfType<HasGameState>();

            if (gameObjectHolder == null)
            {
                Debug.LogError("No game state");
                return;
            }

            var allShips = gameObjectHolder.gameState.ships.Select(ShipState.ToShip).ToList();
            
            var teams = allShips
                .Select(s => s.team)
                .Distinct()
                .ToList();

            foreach (var team in teams)
            {
                var teamPlayer = players.FirstOrDefault(p => p.GetTeam() == team);

                // Compute scores
                var totalScore = Game.GetTeamScore(team, allShips, out var scoreLines);

                // Instantiate column
                var col = Instantiate(teamScoresPrefab, columns);

                var outline = col.transform.Find("Outline").gameObject;
                var playerName = col.transform.Find("Header/PlayerName").gameObject;
                var totalScoreText = col.transform.Find("Score/Value").gameObject;

                outline.SetActive(team == PhotonNetwork.LocalPlayer.GetTeam());
                outline.GetComponent<Image>().color = team.ToColor();
                playerName.GetComponent<TMP_Text>().color = team.ToColor();
                playerName.GetComponent<TMP_Text>().text = teamPlayer?.NickName ?? team.ToString();
                totalScoreText.GetComponent<TMP_Text>().text = totalScore.ToString();
                
                var scoreLinesList = col.transform.Find("List Container/List");
                
                foreach (Transform child in scoreLinesList)
                {
                    Destroy(child.gameObject);
                }

                foreach (var scoreLine in scoreLines)
                {
                    var line = Instantiate(scoreLinePrefab, scoreLinesList);
                    line.transform.Find("Text").GetComponent<TMP_Text>().text = scoreLine.Reason;
                    line.transform.Find("Score").GetComponent<TMP_Text>().text = scoreLine.Score.ToString();
                }
            }
        }
        
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            SceneManager.LoadScene(GameSettings.Default.SceneMainMenu);
        }

        public void ExitToMainMenu()
        {
            PhotonNetwork.LeaveRoom();
        }

    }
}