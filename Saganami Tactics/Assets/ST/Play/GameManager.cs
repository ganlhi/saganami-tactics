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

        private int _expectedShips;

        private int _turn;

        public int Turn
        {
            get => _turn;
            private set
            {
                _turn = value;
                OnTurnChange?.Invoke(this, value);
            }
        }

        private TurnStep _step;

        public TurnStep Step
        {
            get => _step;
            private set
            {
                _step = value;
                OnTurnStepChange?.Invoke(this, value);
            }
        }

        private bool _busy;

        public bool Busy
        {
            get => _busy;
            private set
            {
                _busy = value;
                OnBusyChange?.Invoke(this, value);
            }
        }

        private ShipView _selectedShip;

        public ShipView SelectedShip
        {
            get => _selectedShip;
            set
            {
                _selectedShip = value;
                OnSelectShip?.Invoke(this, value);
            }
        }

        private Dictionary<string, Ssd> _availableSsds;

        public Dictionary<string, Ssd> AvailableSsds
        {
            get
            {
                if (_availableSsds != null) return _availableSsds;
                
                var ssds = Resources.LoadAll<Ssd>("SSD");
                _availableSsds = ssds.ToDictionary((ssd) => ssd.className);

                return _availableSsds;
            }
        }


        public event EventHandler<bool> OnBusyChange;
        public event EventHandler<int> OnTurnChange;
        public event EventHandler<TurnStep> OnTurnStepChange;
        public event EventHandler OnShipsInit;
        public event EventHandler<ShipView> OnSelectShip;

        private void Start()
        {
            InitWhenReady();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            InitWhenReady();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            InitWhenReady();
        }

        private void InitWhenReady()
        {
            if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;
            if (PhotonNetwork.CurrentRoom.MaxPlayers > PhotonNetwork.CurrentRoom.PlayerCount) return;
            Init();
        }

        private void Init()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadStateFromHolder();
                InitShips();

                photonView.RPC("RPC_SetStep", RpcTarget.All, _state.turn, _state.step);
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
            photonView.RPC("RPC_ExpectShips", RpcTarget.All, _state.ships.Count);
            
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
        private void RPC_ExpectShips(int nbShips)
        {
            _expectedShips = nbShips;

            StartCoroutine(WaitForShipsInit());
        }

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

        private void FocusPlayerShip()
        {
            var ship = GetAllShips().First(s => s.OwnedByClient);
            if (ship != null)
            {
                LockCameraToShip(ship);
                SelectedShip = ship;
            }
        }

        public void LockCameraToShip(ShipView ship)
        {
            camera.settings.lockTargetTransform = ship.transform;
            camera.settings.cameraLocked = true;
        }

        private IEnumerator WaitForShipsInit()
        {
            Busy = true;
            do
            {
                yield return null;
            } while (GetAllShips().Count < _expectedShips);

            FocusPlayerShip();
            OnShipsInit?.Invoke(this, EventArgs.Empty);
            
            Busy = false;
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