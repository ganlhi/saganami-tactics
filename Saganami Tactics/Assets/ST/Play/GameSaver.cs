using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(GameManager))]
    public class GameSaver : MonoBehaviourPunCallbacks
    {
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GetComponent<GameManager>();

            if (PhotonNetwork.IsMasterClient)
            {
                _gameManager.OnTurnChange += (sender, i) => SaveGame();

                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    AssociatePlayerToTeam(player);
                }
            }
        }

        private void SaveGame()
        {
            var ships = new List<ShipState>();
            var shipsReports = new List<ShipReports>();

            foreach (var shipView in GameManager.GetAllShips())
            {
                var shipState = ShipState.FromShip(shipView.ship);
                ships.Add(shipState);
                shipsReports.Add(new ShipReports()
                {
                    shipUid = shipView.ship.uid,
                    reports = shipView.GetComponent<ShipLog>().Reports.ToArray()
                });
            }

            var missiles = new List<MissileState>();

            foreach (var missileView in GameManager.GetAllMissiles())
            {
                var missileState = MissileState.FromMissile(missileView.missile);
                missiles.Add(missileState);
            }

            var gameState = new GameState()
            {
                turn = _gameManager.Turn,
                step = _gameManager.Step,
                setup = new GameSetup() {
                    maxCost = _gameManager.GameSetup.maxCost,
                    nbPlayers = _gameManager.GameSetup.nbPlayers,
                    bluePlayer = PhotonNetwork.CurrentRoom.BluePlayerName(),
                    yellowPlayer = PhotonNetwork.CurrentRoom.YellowPlayerName(),
                    greenPlayer = PhotonNetwork.CurrentRoom.GreenPlayerName(),
                    magentaPlayer = PhotonNetwork.CurrentRoom.MagentaPlayerName(),
                },
                ships = ships,
                missiles = missiles,
                shipsReports = shipsReports,
            };

            GameStateSaveSystem.Save(gameState, PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (changedProps.ContainsKey(GameSettings.Default.ColorIndexProp))
            {
                AssociatePlayerToTeam(targetPlayer);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            AssociatePlayerToTeam(newPlayer);
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
        }

        private void AssociatePlayerToTeam(Player player)
        {
            Debug.Log($"AssociatePlayerToTeam {player.NickName} {player.GetTeam()}");
            string playerTeamProp;
            switch (player.GetTeam())
            {
                case Team.Blue:
                    playerTeamProp = GameSettings.Default.BluePlayerProp;
                    break;
                case Team.Yellow:
                    playerTeamProp = GameSettings.Default.YellowPlayerProp;
                    break;
                case Team.Green:
                    playerTeamProp = GameSettings.Default.GreenPlayerProp;
                    break;
                case Team.Magenta:
                    playerTeamProp = GameSettings.Default.MagentaPlayerProp;
                    break;
                default:
                    return;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable()
            {
                {playerTeamProp, player.NickName}
            });
        }
    }
}