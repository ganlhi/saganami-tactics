using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Scriptable;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ST.Play
{
    [RequireComponent(typeof(PhotonView))]
    public class GameManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
        [SerializeField] private new Moba_Camera camera;
#pragma warning restore 649
        
        public int Turn { get; private set; }
        public TurnStep Step { get; private set; }
        public bool Busy { get; private set; }
        
        private void Start()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                Init();
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Init();
        }

        private void Init()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadStateFromHolder();
                InitShips();

                photonView.RPC("RPC_SetStep", RpcTarget.All, _state.turn, _state.step);
                photonView.RPC("RPC_FocusPlayerShip", RpcTarget.All);
                OnStepStart(_state.turn, _state.step);
            }
        }

        #region MasterClient

#pragma warning disable 0649
        [SerializeField] private GameStateHolder stateHolder;
#pragma warning restore 0649
        private GameState _state;

        private void LoadStateFromHolder()
        {
            if (stateHolder != null)
            {
                _state = Instantiate(stateHolder).state;
            }
        }

        private void InitShips()
        {
            foreach (var shipState in _state.ships)
            {
                var ship = ShipState.ToShip(shipState);
                
                var sv = PhotonNetwork
                    .InstantiateSceneObject("Prefabs/ShipView", ship.position, ship.rotation)
                    .GetComponent<ShipView>();

                sv.ship = ship;
            }
        }

        public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.Players.Values.All(p => p.IsReady()))
            {
                NextStep();
            }
        }

        private void NextStep()
        {
            PhotonNetwork.CurrentRoom.ResetPlayersReadiness();
            Game.NextStep(Turn, Step, out var nextTurn, out var nextStep);

            if (nextTurn > 1 || nextStep != TurnStep.Start)
            {
                OnStepEnd(Turn, Step);
            }

            photonView.RPC("RPC_SetStep", RpcTarget.All, nextTurn, nextStep);

            OnStepStart(nextTurn, nextStep);
        }
        
        private void OnStepEnd(int turn, TurnStep step)
        {
            var gameEvents = Game.OnStepEnd(turn, step);
            ProcessEvents(gameEvents);
        }

        private void OnStepStart(int turn, TurnStep step)
        {
            var gameEvents = Game.OnStepStart(turn, step);
            ProcessEvents(gameEvents);
        }

        private void ProcessEvents(IEnumerable<GameEvent> gameEvents)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            foreach (var gameEvent in gameEvents)
            {
                switch (gameEvent)
                {
                    case GameEvent.UpdateShipsFutureMovement:
                        GetAllShips().ForEach(shipView => shipView.UpdateFutureMovement());
                        break;
                    case GameEvent.MoveShipsToMarkers:
                        StartCoroutine(MoveShipsToMarkers());
                        break;
                    case GameEvent.PlaceShipsMarkers:
                        GetAllShips().ForEach(shipView => shipView.PlaceMarker());
                        break;
                    case GameEvent.ResetThrustAndPlottings:
                        GetAllShips().ForEach(shipView => shipView.ResetThrustAndPlottings());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerator MoveShipsToMarkers()
        {
            var ships = GetAllShips();
            
            ships.ForEach(shipView => shipView.ApplyMovement());
            
            do
            {
                yield return null;
            } while (ships.Any(s => !s.ReadyToMove));

            photonView.RPC("RPC_AnimateMoveShipsToMarkers", RpcTarget.All);
        }

        #endregion MasterClient

        #region AllClients

        [PunRPC]
        private void RPC_SetStep(int turn, TurnStep step)
        {
            Turn = turn;
            Step = step;
        }
        
        [PunRPC]
        private void RPC_AnimateMoveShipsToMarkers()
        {
            StartCoroutine(AnimateMoveShipsToMarkers());
        }
        
        [PunRPC]
        private void RPC_FocusPlayerShip()
        {
            var ship = GetAllShips().First(s => s.OwnedByClient);
            if (ship != null)
            {
                LockCameraToShip(ship);
            }
        }

        public void LockCameraToShip(ShipView ship)
        {
            camera.settings.lockTargetTransform = ship.transform;
            camera.settings.cameraLocked = true;
        }

        private IEnumerator AnimateMoveShipsToMarkers()
        {
            Busy = true;
            var ships = GetAllShips();
        
            ships.ForEach(shipView => shipView.AutoMove());
        
            do
            {
                yield return null;
            } while (ships.Any(s => s.Busy));

            SetReady(true);
            Busy = false;
        }
        
        public List<ShipView> GetAllShips()
        {
            return PhotonNetwork
                .FindGameObjectsWithComponent(typeof(ShipView))
                .Select(go => go.GetComponent<ShipView>())
                .ToList();
        }

        public void SetReady(bool ready)
        {
            PhotonNetwork.LocalPlayer.SetReady(ready);
        }

        #endregion AllClients
    }
}