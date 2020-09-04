using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Play;
using ST.Scriptable;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ST.Deploy
{
    [RequireComponent(typeof(PhotonView))]
    public class DeployManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
#pragma warning disable 108,114
        [SerializeField] private Moba_Camera camera;
        [SerializeField] private GameStateHolder stateHolder;
        [SerializeField] private GameObject blueZone;
        [SerializeField] private GameObject yellowZone;
        [SerializeField] private GameObject greenZone;
        [SerializeField] private GameObject magentaZone;
        [SerializeField] private CanvasGroup deployPanels;
#pragma warning restore 108,114
#pragma warning restore 649

        private int _clientsReadyToContinue;
        private int _expectedShips;

        private GameState _state;

        private ShipView _selectedShip;

        private Vector3 _fwdDirection;
        private Vector3 _rightDirection;

        public ShipView SelectedShip
        {
            get => _selectedShip;
            set
            {
                if (_selectedShip == value) return;
                var previous = _selectedShip;
                _selectedShip = value;
                OnSelectShip?.Invoke(this, new Tuple<ShipView, ShipView>(value, previous));
            }
        }

        public event EventHandler<Tuple<ShipView, ShipView>> OnSelectShip;
        public event EventHandler OnShipsInit;

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
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(Init());
            }
        }

        private IEnumerator Init()
        {
            LoadStateFromHolder();
            SetZonesVisibility();

            _clientsReadyToContinue = 0;

            InitShips();

            do
            {
                yield return null;
            } while (_clientsReadyToContinue < PhotonNetwork.CurrentRoom.PlayerCount);

            GetAllShips().ForEach(s => s.PlaceMarker());
        }

        #region MasterClient

        private void LoadStateFromHolder()
        {
            var gameObjectHolder = FindObjectOfType<HasGameState>();

            if (gameObjectHolder != null)
            {
                _state = gameObjectHolder.gameState;
            }
            else if (stateHolder != null)
            {
                _state = Instantiate(stateHolder).state;
            }
        }

        private void SetZonesVisibility()
        {
            photonView.RPC("RPC_SetZonesVisibility", RpcTarget.All,
                _state.ships.Any(s => s.team == Team.Blue),
                _state.ships.Any(s => s.team == Team.Yellow),
                _state.ships.Any(s => s.team == Team.Green),
                _state.ships.Any(s => s.team == Team.Magenta)
            );
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

        [PunRPC]
        private void RPC_ReadyToContinue()
        {
            _clientsReadyToContinue += 1;
        }

        [PunRPC]
        private void RPC_MoveShip(string uid, Vector3 movement)
        {
            var shipView = GetShipById(uid);
            if (shipView == null) return;

            var newPosition = shipView.ship.position + movement;

            // Is it in zone?
            var spawnPoint = Game.GetTeamSpawnPoint(shipView.ship.team);
            var diffX = Math.Abs(spawnPoint.x - newPosition.x);
            var diffY = Math.Abs(spawnPoint.y - newPosition.y);
            var diffZ = Math.Abs(spawnPoint.z - newPosition.z);

            if (diffX > 5 || diffY > 5 || diffZ > 5) return;

            // Is there another ship in this position?
            if (GetAllShips().Any(s => s.ship.uid != uid && s.ship.position == newPosition)) return;

            // Set position
            shipView.ship.position = newPosition;
            shipView.transform.position = newPosition;
            shipView.PlaceMarker();
        }

        [PunRPC]
        private void RPC_AccelerateShip(string uid, Vector3 acc)
        {
            var shipView = GetShipById(uid);
            if (shipView == null) return;

            var newVelocity = shipView.ship.velocity + acc;

            if (newVelocity.magnitude > 10) return;

            shipView.ship.velocity = newVelocity;
            shipView.PlaceMarker();
        }

        [PunRPC]
        private void RPC_ChangeShipAttitude(string uid, PlottingAction action, int value)
        {
            var shipView = GetShipById(uid);
            if (shipView == null) return;

            Vector3 rotationAxis;
            switch (action)
            {
                case PlottingAction.Yaw:
                    rotationAxis = Vector3.up;
                    break;
                case PlottingAction.Pitch:
                    rotationAxis = Vector3.right;
                    break;
                case PlottingAction.Roll:
                    rotationAxis = Vector3.forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            var newRotation = shipView.ship.rotation * Quaternion.AngleAxis(30f * value, rotationAxis);
            shipView.ship.rotation = newRotation;
            shipView.ship.endMarkerRotation = newRotation;
            shipView.transform.rotation = newRotation;
            shipView.PlaceMarker();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (PhotonNetwork.IsMasterClient &&
                PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers &&
                PhotonNetwork.CurrentRoom.Players.Values.All(p => p.IsReady()))
            {
                CreateGameStateAndContinue();
            }
        }

        private void CreateGameStateAndContinue()
        {
            var allShips = new List<ShipState>();

            foreach (var shipView in GetAllShips())
            {
                var shipState = ShipState.FromShip(shipView.ship);
                allShips.Add(shipState);
            }

            var gameState = new GameState()
            {
                turn = 1,
                step = TurnStep.Plotting,
                setup = new GameSetup()
                {
                    maxCost = PhotonNetwork.CurrentRoom.GetMaxPoints(),
                    nbPlayers = PhotonNetwork.CurrentRoom.PlayerCount
                },
                ships = allShips
            };


            var gameStateContainer = FindObjectOfType<HasGameState>();

            if (gameStateContainer == null)
            {
                var gameStateGo = new GameObject {name = "_GameState"};
                DontDestroyOnLoad(gameStateGo);
                gameStateContainer = gameStateGo.AddComponent<HasGameState>();
            }

            gameStateContainer.gameState = gameState;
            PhotonNetwork.LoadLevel(GameSettings.Default.ScenePlay);
        }

        #endregion MasterClient

        #region AllClients

        public void SetReadyToPlay()
        {
            PhotonNetwork.LocalPlayer.SetReady(true);
            deployPanels.alpha = 0;
        }

        private void FocusPlayerShip()
        {
            var ship = GetAllShips().First(s => s.OwnedByClient);
            if (ship == null) return;
            LockCameraToShip(ship);
            SelectedShip = ship;
        }

        public void LockCameraToShip(ShipView ship)
        {
            camera.settings.lockTargetTransform = ship.transform;
            camera.settings.cameraLocked = true;
        }

        public void LockCameraToSelectedShip()
        {
            LockCameraToShip(_selectedShip);
        }

        [PunRPC]
        private void RPC_ExpectShips(int nbShips)
        {
            _expectedShips = nbShips;

            StartCoroutine(WaitForShipsInit());
        }

        [PunRPC]
        private void RPC_SetZonesVisibility(bool blue, bool yellow, bool green, bool magenta)
        {
            blueZone.SetActive(blue);
            yellowZone.SetActive(yellow);
            greenZone.SetActive(green);
            magentaZone.SetActive(magenta);
        }

        private IEnumerator WaitForShipsInit()
        {
            do
            {
                yield return null;
            } while (GetAllShips().Count < _expectedShips);

            GetAllShips().ForEach(sv =>
            {
                sv.PlaceMarker();
                if (!sv.OwnedByClient)
                {
                    Utils.MoveToLayer(sv.gameObject.transform, LayerMask.NameToLayer("OtherPlayers"));
                }
            });
            FocusPlayerShip();
            OnShipsInit?.Invoke(this, EventArgs.Empty);

            var team = PhotonNetwork.LocalPlayer.GetTeam();
            if (team.HasValue)
            {
                Game.GetTeamSpawnPoint(team.Value,
                    out var fwdDirection,
                    out var rightDirection);

                _fwdDirection = fwdDirection;
                _rightDirection = rightDirection;
            }

            photonView.RPC("RPC_ReadyToContinue", RpcTarget.MasterClient);
        }

        public static List<ShipView> GetAllShips()
        {
            return PhotonNetwork
                .FindGameObjectsWithComponent(typeof(ShipView))
                .Select(go => go.GetComponent<ShipView>())
                .ToList();
        }

        public static List<ShipView> GetPlayerShips()
        {
            return GetAllShips().Where(s => s.ship.team == PhotonNetwork.LocalPlayer.GetTeam()).ToList();
        }

        [CanBeNull]
        public static ShipView GetShipById(string shipId)
        {
            return GetAllShips().First(s => s.ship.uid == shipId);
        }

        #region Deployment

        public void MoveForward()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, _fwdDirection);
        }

        public void MoveBackward()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, -_fwdDirection);
        }

        public void MoveLeft()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, -_rightDirection);
        }

        public void MoveRight()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, _rightDirection);
        }

        public void MoveUp()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, Vector3.up);
        }

        public void MoveDown()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_MoveShip", RpcTarget.MasterClient, _selectedShip.ship.uid, Vector3.down);
        }

        public void PitchUp()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid,
                PlottingAction.Pitch, -1);
        }

        public void PitchDown()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid,
                PlottingAction.Pitch, 1);
        }

        public void YawLeft()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid, PlottingAction.Yaw,
                -1);
        }

        public void YawRight()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid, PlottingAction.Yaw,
                1);
        }

        public void RollLeft()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid,
                PlottingAction.Roll, 1);
        }

        public void RollRight()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_ChangeShipAttitude", RpcTarget.MasterClient, _selectedShip.ship.uid,
                PlottingAction.Roll, -1);
        }

        public void AccelerateForward()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, _fwdDirection);
        }

        public void AccelerateBackward()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, -_fwdDirection);
        }

        public void AccelerateLeft()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, -_rightDirection);
        }

        public void AccelerateRight()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, _rightDirection);
        }

        public void AccelerateUp()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, Vector3.up);
        }

        public void AccelerateDown()
        {
            if (_selectedShip == null) return;
            photonView.RPC("RPC_AccelerateShip", RpcTarget.MasterClient, _selectedShip.ship.uid, Vector3.down);
        }

        #endregion Deployment

        #endregion AllClients
    }
}