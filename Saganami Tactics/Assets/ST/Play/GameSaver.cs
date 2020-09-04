using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(GameManager))]
    public class GameSaver : MonoBehaviour
    {
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GetComponent<GameManager>();

            if (PhotonNetwork.IsMasterClient)
            {
                _gameManager.OnTurnChange += (sender, i) => SaveGame();
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
                setup = _gameManager.GameSetup,
                ships = ships,
                missiles = missiles,
                shipsReports = shipsReports,
            };

            GameStateSaveSystem.Save(gameState, PhotonNetwork.CurrentRoom.Name);
        }
    }
}