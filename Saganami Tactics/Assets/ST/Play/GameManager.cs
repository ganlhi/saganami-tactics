using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using ST.Common;
using ST.Scriptable;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ST.Play
{
    [RequireComponent(typeof(PhotonView))]
    public class GameManager : MonoBehaviourPunCallbacks
    {
#pragma warning disable 649
#pragma warning disable 108,114
        [SerializeField] private Moba_Camera camera;
#pragma warning restore 108,114
#pragma warning restore 649

        private int _expectedShips;
        private int _expectedMissiles;

        private bool _shouldGoToMainMenuOnLeftRoom;

        private int _turn;

        public int Turn
        {
            get => _turn;
            private set
            {
                if (_turn == value) return;
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
                if (_step == value) return;
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
                if (_selectedShip == value) return;
                var previous = _selectedShip;
                _selectedShip = value;
                OnSelectShip?.Invoke(this, new Tuple<ShipView, ShipView>(value, previous));
            }
        }

        public event EventHandler<bool> OnBusyChange;
        public event EventHandler<int> OnTurnChange;
        public event EventHandler<TurnStep> OnTurnStepChange;
        public event EventHandler OnShipsInit;
        public event EventHandler<Tuple<ShipView, ShipView>> OnSelectShip;
        public event EventHandler OnTargetsIdentified;
        public event EventHandler OnShipStatusChanged;


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

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            if (_shouldGoToMainMenuOnLeftRoom)
                SceneManager.LoadScene(GameSettings.Default.SceneMainMenu);
        }

        public void ExitToMainMenu()
        {
            _shouldGoToMainMenuOnLeftRoom = true;
            PhotonNetwork.LeaveRoom();
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

            _clientsReadyToContinue = 0;

            InitShipsAndMissiles();

            do
            {
                yield return null;
            } while (_clientsReadyToContinue < PhotonNetwork.CurrentRoom.PlayerCount);

            photonView.RPC("RPC_SetStep", RpcTarget.All, _state.turn, _state.step);
            OnStepStart(_state.turn, _state.step);
        }

/*
        // FOR TESTING PURPOSES         
        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (Input.GetKeyDown(KeyCode.End) && SelectedShip != null)
            {
//                _pendingDestroyedAmmo.Add(SelectedShip, new List<Tuple<int, int>>()
//                {
//                    new Tuple<int, int>(0, 3),
//                    new Tuple<int, int>(0, 2),
//                });
//                
//                DispatchPendingDestroyedAmmo();

                _pendingAlterations.Add(SelectedShip, new List<SsdAlteration>()
                {
//                    new SsdAlteration() {type = SsdAlterationType.Movement, slotType = HitLocationSlotType.None},
                    new SsdAlteration()
                    {
                        type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Bridge,
                        location = 1 + (int) Array.FindIndex(SelectedShip.ship.Ssd.hitLocations,
                                       loc => loc.slots.Any(s => s.type == HitLocationSlotType.Bridge))
                    },
//                    new SsdAlteration() {type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Missile},
//                    new SsdAlteration()
//                        {type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Hull},
//                    new SsdAlteration()
//                        {type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Hull},
//                    new SsdAlteration()
//                        {type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Hull},
                });
//                _pendingAlterations.Add(SelectedShip, Game.MakeAlterationsForBoxes(
//                    new SsdAlteration()
//                        {type = SsdAlterationType.Slot, slotType = HitLocationSlotType.Hull},
//                    3,
//                    SelectedShip.ship.Ssd.hull,
//                    SelectedShip.ship.alterations,
//                    new List<SsdAlteration>()
//                ));

                DispatchPendingAlterations();
            }
        }
*/

        #region MasterClient

#pragma warning disable 0649
        [SerializeField] private GameStateHolder stateHolder;
#pragma warning restore 0649

        private GameState _state;
        private int _clientsReadyToContinue;
        private readonly Dictionary<ShipView, List<Report>> _pendingReports = new Dictionary<ShipView, List<Report>>();

        private readonly Dictionary<ShipView, List<SsdAlteration>> _pendingAlterations =
            new Dictionary<ShipView, List<SsdAlteration>>();

        private readonly Dictionary<ShipView, List<Tuple<int, int>>> _pendingDestroyedAmmo =
            new Dictionary<ShipView, List<Tuple<int, int>>>();

        public GameSetup GameSetup => _state.setup;

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

        private void InitShipsAndMissiles()
        {
            photonView.RPC("RPC_ExpectShipsAndMissiles", RpcTarget.All,
                _state.ships?.Count ?? 0,
                _state.missiles?.Count ?? 0);

            if (_state.ships != null)
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

            if (_state.missiles != null)
            {
                foreach (var missileState in _state.missiles)
                {
                    var missile = MissileState.ToMissile(missileState);

                    var mv = PhotonNetwork
                        .InstantiateSceneObject("Prefabs/MissileView", missile.position, missile.rotation)
                        .GetComponent<MissileView>();

                    mv.missile = missile;
                }
            }

            if (_state.shipsReports != null)
            {
                foreach (var shipReports in _state.shipsReports)
                {
                    var shipView = GetShipById(shipReports.shipUid);
                    if (shipView == null) continue;
                    shipView.GetComponent<ShipLog>().AddReports(shipReports.reports.ToList());
                }
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

            if (nextTurn > 1 || nextStep != TurnStep.Plotting)
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
                        photonView.RPC("RPC_MoveShipsToMarkers", RpcTarget.All);
                        break;
                    case GameEvent.PlaceShipsMarkers:
                        GetAllShips().ForEach(shipView => shipView.PlaceMarker());
                        break;
                    case GameEvent.ResetThrustAndPlottings:
                        GetAllShips().ForEach(shipView => shipView.ResetThrustAndPlottings());
                        break;
                    case GameEvent.IdentifyTargets:
                        photonView.RPC("RPC_IdentifyTargets", RpcTarget.All);
                        break;
                    case GameEvent.ClearTargets:
                        photonView.RPC("RPC_ClearTargets", RpcTarget.All);
                        break;
                    case GameEvent.FireMissiles:
                        FireMissiles();
                        break;
                    case GameEvent.UpdateMissiles:
                        UpdateMissiles();
                        break;
                    case GameEvent.MoveMissiles:
                        StartCoroutine(MoveAndDestroyMissiles());
                        break;
                    case GameEvent.FireBeams:
                        FireBeams();
                        break;
                    case GameEvent.ResetRepairAttempts:
                        GetAllShips().ForEach(shipView => shipView.ResetRepairAttempts());
                        break;
                    case GameEvent.ResetDeployedDecoys:
                        GetAllShips().ForEach(shipView => shipView.ResetDeployedDecoys());
                        break;
                    case GameEvent.CheckEndGame:
                        CheckEndGame();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CheckEndGame()
        {
            if (Game.ShouldEndGame(GetAllShips().Select(s => s.ship)))
            {
                var allShips = new List<ShipState>();

                foreach (var shipView in GetAllShips())
                {
                    var shipState = ShipState.FromShip(shipView.ship);
                    allShips.Add(shipState);
                }

                var gameState = new GameState()
                {
                    turn = _turn,
                    step = _step,
                    setup = _state.setup,
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

                PhotonNetwork.LoadLevel(GameSettings.Default.SceneEndGame);
            }
        }

        private void PopulateAttackPendingData(ShipView target,
            List<Tuple<ReportType, string>> reports,
            List<Tuple<int, int>> pendingDestroyedAmmo)
        {
            // Reports
            if (reports.Any())
            {
                var pendingReports = reports.Select(t => new Report()
                {
                    turn = Turn,
                    type = t.Item1,
                    message = t.Item2,
                });
                if (_pendingReports.ContainsKey(target))
                    _pendingReports[target].AddRange(pendingReports);
                else
                    _pendingReports.Add(target, pendingReports.ToList());
            }

            // Destroyed ammo
            if (pendingDestroyedAmmo.Any())
            {
                if (_pendingDestroyedAmmo.ContainsKey(target))
                    _pendingDestroyedAmmo[target].AddRange(pendingDestroyedAmmo);
                else
                    _pendingDestroyedAmmo.Add(target, pendingDestroyedAmmo.ToList());
            }
        }

        private void FireMissiles()
        {
            GetAllShips().ForEach(shipView =>
            {
                var fcon = shipView.GetComponent<FireControl>();

                foreach (var targetingContext in fcon.Locks.Values)
                {
                    if (!WeaponHelper.IsProjectileWeapon(targetingContext.Mount.model)) continue;

                    var missile = new Missile(shipView.ship, targetingContext, Turn);

                    var mv = PhotonNetwork
                        .InstantiateSceneObject("Prefabs/MissileView", missile.position, missile.rotation)
                        .GetComponent<MissileView>();

                    mv.missile = missile;

                    shipView.ConsumeAmmo(targetingContext.Mount, targetingContext.Number);
                }
            });
        }

        private void FireBeams()
        {
            var beamAnims = new List<Tuple<Vector3, Vector3>>();

            GetAllShips().ForEach(shipView =>
            {
                var fcon = shipView.GetComponent<FireControl>();

                foreach (var targetingContext in fcon.Locks.Values)
                {
                    if (!WeaponHelper.IsBeamWeapon(targetingContext.Mount.model)) continue;
                    var target = GetShipById(targetingContext.Target.uid);

                    if (target == null || target.ship.Status != ShipStatus.Ok) continue;

                    var reports = new List<Tuple<ReportType, string>>();
                    var pendingDestroyedAmmo = new List<Tuple<int, int>>();

                    if (!_pendingAlterations.ContainsKey(target))
                    {
                        _pendingAlterations.Add(target, new List<SsdAlteration>());
                    }

                    var pendingAlterations = _pendingAlterations[target];

                    var animTargetPos = Game.FireBeam(targetingContext, shipView.ship, target.ship,
                        ref reports,
                        ref pendingAlterations,
                        ref pendingDestroyedAmmo);

                    beamAnims.Add(new Tuple<Vector3, Vector3>(shipView.ship.position, animTargetPos));

                    PopulateAttackPendingData(target, reports, pendingDestroyedAmmo);
                }
            });

            StartCoroutine(AnimateBeams(beamAnims));
        }

        private void UpdateMissiles()
        {
            var missileViews = GetAllMissiles();

            missileViews.ForEach(missileView =>
            {
                var reports = new List<Tuple<ReportType, string>>();

                var attacker = GetShipById(missileView.missile.attackerId);
                var target = GetShipById(missileView.missile.targetId);
                if (attacker == null || target == null) return;

                var missile = Game.UpdateMissile(missileView.missile, attacker.ship, target.ship, Turn,
                    ref reports);
                missileView.UpdateMissile(missile);

                if (!_pendingAlterations.ContainsKey(target))
                {
                    _pendingAlterations.Add(target, new List<SsdAlteration>());
                }

                var pendingAlterations = _pendingAlterations[target];
                var pendingDestroyedAmmo = new List<Tuple<int, int>>();

                if (missile.status == MissileStatus.Hitting && missile.number > 0)
                {
                    Game.HitTarget(missile.weapon, missile.hitSide, missile.number, missile.attackRange, attacker.ship,
                        target.ship,
                        ref reports,
                        ref pendingAlterations,
                        ref pendingDestroyedAmmo);
                }

                PopulateAttackPendingData(target, reports, pendingDestroyedAmmo);
            });

            photonView.RPC("RPC_WaitForMissilesUpdates", RpcTarget.All, missileViews.Count);
        }

        private IEnumerator MoveAndDestroyMissiles()
        {
            _clientsReadyToContinue = 0;

            photonView.RPC("RPC_MoveMissiles", RpcTarget.All);

            do
            {
                yield return null;
            } while (_clientsReadyToContinue < PhotonNetwork.CurrentRoom.PlayerCount);

            DispatchPendingReports();
            DispatchPendingAlterations();
            DispatchPendingDestroyedAmmo();
            DestroyMissiles();
        }

        private IEnumerator AnimateBeams(List<Tuple<Vector3, Vector3>> beamAnims)
        {
            _clientsReadyToContinue = 0;

            var beamAnimsAsObjects = new Vector3[beamAnims.Count * 2];
            var i = 0;
            foreach (var (from, to) in beamAnims)
            {
                beamAnimsAsObjects[i] = from;
                beamAnimsAsObjects[i + 1] = to;
                i += 2;
            }

            photonView.RPC("RPC_WaitForBeamsAnimation", RpcTarget.All, beamAnimsAsObjects);

            do
            {
                yield return null;
            } while (_clientsReadyToContinue < PhotonNetwork.CurrentRoom.PlayerCount);

            DispatchPendingReports();
            DispatchPendingAlterations();
            DispatchPendingDestroyedAmmo();
        }

        private void DestroyMissiles()
        {
            GetAllMissiles().ForEach(missileView =>
            {
                if (missileView.missile.status == MissileStatus.Destroyed ||
                    missileView.missile.status == MissileStatus.Hitting ||
                    missileView.missile.status == MissileStatus.Missed)
                {
                    PhotonNetwork.Destroy(missileView.gameObject);
                }
            });
        }

        private void DispatchPendingReports()
        {
            foreach (var kv in _pendingReports)
            {
                kv.Key.GetComponent<ShipLog>().AddReports(kv.Value);
            }

            _pendingReports.Clear();
        }

        private void DispatchPendingAlterations()
        {
            foreach (var kv in _pendingAlterations)
            {
                kv.Key.AddAlterations(kv.Value);
            }

            _pendingAlterations.Clear();

            CheckShipsForDestruction();
        }

        private void DispatchPendingDestroyedAmmo()
        {
            foreach (var kv in _pendingDestroyedAmmo)
            {
                var shipView = kv.Key;
                var accumulator = new Dictionary<WeaponMount, int>();

                foreach (var (mountIndex, amount) in kv.Value)
                {
                    var weaponMount = shipView.ship.Ssd.weaponMounts[mountIndex];

                    if (accumulator.ContainsKey(weaponMount))
                    {
                        accumulator[weaponMount] += amount;
                    }
                    else
                    {
                        accumulator.Add(weaponMount, amount);
                    }
                }

                shipView.ConsumeAmmos(accumulator);
            }

            _pendingDestroyedAmmo.Clear();
        }

        private void CheckShipsForDestruction()
        {
            GetAllShips().ForEach(shipView =>
            {
                if (shipView.ship.Status == ShipStatus.Destroyed) return;

                var remainingStructuralPoints =
                    SsdHelper.GetRemainingStructuralPoints(shipView.ship.Ssd, shipView.ship.alterations);

                if (remainingStructuralPoints == 0)
                {
                    shipView.DestroyShip();
                    shipView.GetComponent<ShipLog>().AddReport(new Report()
                    {
                        turn = Turn,
                        type = ReportType.ShipDestroyed,
                        message = $"{shipView.ship.name} has been destroyed!"
                    });

//                    if (shipView == SelectedShip)
//                    {
//                        // Select next ship from player
//                        var nextPlayerShip = GetPlayerShips().FirstOrDefault(s => s.ship.Status == ShipStatus.Ok);
//                        if (nextPlayerShip != null)
//                        {
//                            SelectedShip = nextPlayerShip;
//                        }
//                        else
//                        {
//                            SelectedShip = GetAllShips().FirstOrDefault(s => s.ship.Status == ShipStatus.Ok);
//                        }
//                    }
                }
            });
        }

        [PunRPC]
        private void RPC_ReadyToContinue()
        {
            _clientsReadyToContinue += 1;
        }

        [PunRPC]
        private void RPC_AttemptRepair(string shipId, int location, Side side, SsdAlterationType type,
            HitLocationSlotType slotType)
        {
            var shipView = GetShipById(shipId);

            if (shipView == null)
            {
                Debug.LogError("Cannot find ship with ID " + shipId);
                return;
            }

            if (Game.AttemptCrewRateCheck(shipView.ship))
            {
                shipView.AddRepairAttempt(true);
                shipView.RemoveAlteration(location, side, type, slotType);

                if (slotType == HitLocationSlotType.ForwardImpeller || slotType == HitLocationSlotType.AftImpeller)
                {
                    // Also repair movement
                    shipView.RemoveAlteration(location, side, SsdAlterationType.Movement, HitLocationSlotType.None);
                }

                shipView.GetComponent<ShipLog>().AddReport(new Report()
                {
                    turn = Turn,
                    type = ReportType.Info,
                    message = $"Successful repair: {DescribeAlteration(location, side, type, slotType)}"
                });
            }
            else
            {
                shipView.AddRepairAttempt(false);
                shipView.SetAlterationDestroyed(location, side, type, slotType);
                shipView.GetComponent<ShipLog>().AddReport(new Report()
                {
                    turn = Turn,
                    type = ReportType.DamageTaken,
                    message = $"Failed repair: {DescribeAlteration(location, side, type, slotType)}"
                });
            }
        }

        private string DescribeAlteration(int location, Side side, SsdAlterationType type, HitLocationSlotType slotType)
        {
            switch (type)
            {
                case SsdAlterationType.Slot:
                    switch (slotType)
                    {
                        case HitLocationSlotType.Missile:
                        case HitLocationSlotType.Laser:
                        case HitLocationSlotType.Graser:
                        case HitLocationSlotType.CounterMissile:
                        case HitLocationSlotType.PointDefense:
                            return $"{side.ToFriendlyString()} {SsdHelper.SlotTypeToString(slotType)}";
                        case HitLocationSlotType.Hull:
                            return "hull";
                        default:
                            return $"{SsdHelper.SlotTypeToString(slotType)} (location {location})";
                    }

                case SsdAlterationType.Movement:
                    return "movement";
                case SsdAlterationType.Sidewall:
                    return $"{side.ToFriendlyString()} sidewall";
                case SsdAlterationType.Structural:
                    return "structural integrity";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion MasterClient

        #region AllClients

#pragma warning disable 0649
        [SerializeField] private BeamsLines beamsLines;
#pragma warning restore 0649

        [PunRPC]
        private void RPC_ExpectShipsAndMissiles(int nbShips, int nbMissiles)
        {
            _expectedShips = nbShips;
            _expectedMissiles = nbMissiles;

            StartCoroutine(WaitForShipsAndMissilesInit());
        }

        [PunRPC]
        private void RPC_SetStep(int turn, TurnStep step)
        {
            if (turn != Turn)
            {
                // wait for broadcast new turn
                _turn = turn;
                Step = step;
                OnTurnChange?.Invoke(this, _turn);
            }
            else
            {
                Step = step;
            }

            SetFutureVelocityVectorsVisibility();
        }

        private void SetFutureVelocityVectorsVisibility()
        {
            GetAllShips().ForEach(sv =>
                sv.GetComponent<ShipFutureVelocityVector>().visible =
                    Step == TurnStep.Plotting && sv.ship.Status == ShipStatus.Ok);
        }

        [PunRPC]
        private void RPC_MoveShipsToMarkers()
        {
            StartCoroutine(AnimateMoveShipsToMarkers());
        }

        [PunRPC]
        private void RPC_IdentifyTargets()
        {
            var playerShips = GetPlayerShips();
            var allShips = GetAllShips().Select(s => s.ship).ToList();
            foreach (var shipView in playerShips)
            {
                var potentialTargets = Game.IdentifyTargets(shipView.ship, allShips);
                shipView.GetComponent<FireControl>().PotentialTargets = potentialTargets;
            }

            OnTargetsIdentified?.Invoke(this, EventArgs.Empty);
        }

        [PunRPC]
        private void RPC_ClearTargets()
        {
            GetPlayerShips().ForEach(s => s.GetComponent<FireControl>().Clear());
        }

        [PunRPC]
        private void RPC_WaitForMissilesUpdates(int nbMissiles)
        {
            StartCoroutine(WaitForMissilesUpdates(nbMissiles));
        }

        [PunRPC]
        private void RPC_WaitForBeamsAnimation(Vector3[] animPositionsTuples)
        {
            var animPositions = new List<Tuple<Vector3, Vector3>>();

            for (var i = 0; i < animPositionsTuples.Length; i += 2)
            {
                var from = animPositionsTuples[i];
                var to = animPositionsTuples[i + 1];
                animPositions.Add(new Tuple<Vector3, Vector3>(from, to));
            }

            StartCoroutine(WaitForBeamsAnimation(animPositions));
        }

        [PunRPC]
        private void RPC_MoveMissiles()
        {
            StartCoroutine(AnimateMoveMissilesToNextPosition());
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

        public void LockCameraToShipMarker(ShipMarker shipMarker)
        {
            camera.settings.lockTargetTransform = shipMarker.transform;
            camera.settings.cameraLocked = true;
        }

        public void LockCameraToShipOrMarker(ShipView ship)
        {
            if (camera.settings.cameraLocked)
            {
                if (camera.settings.lockTargetTransform == ship.transform)
                {
                    LockCameraToShipMarker(ship.EndMarker);
                }
                else
                {
                    LockCameraToShip(ship);
                }
            }
            else
            {
                LockCameraToShip(ship);
            }
        }

        public void LockCameraToMissile(MissileView missile)
        {
            camera.settings.lockTargetTransform = missile.transform;
            camera.settings.cameraLocked = true;
        }

        public void LockCameraToSelectedShip()
        {
            LockCameraToShipOrMarker(_selectedShip);
        }

        private IEnumerator WaitForShipsAndMissilesInit()
        {
            Busy = true;
            do
            {
                yield return null;
            } while (GetAllShips().Count < _expectedShips || GetAllMissiles().Count < _expectedMissiles);

            GetAllShips().ForEach(sv => sv.PlaceMarker());
            FocusPlayerShip();
            OnShipsInit?.Invoke(this, EventArgs.Empty);

            Busy = false;
            photonView.RPC("RPC_ReadyToContinue", RpcTarget.MasterClient);
        }

        private IEnumerator AnimateMoveShipsToMarkers()
        {
            var ships = GetAllShips()
                .Where(s => s.ship.Status == ShipStatus.Ok || s.ship.Status == ShipStatus.Surrendered)
                .ToList();

            Busy = true;

            if (ships.Any(s =>
                s.ship.endMarkerPosition != s.ship.position || s.ship.endMarkerRotation != s.ship.rotation))
            {
                ships.ForEach(shipView => shipView.AutoMove());

                do
                {
                    yield return null;
                } while (ships.Any(s => s.Busy));
            }
            else
            {
                yield return new WaitForSeconds(.5f);
            }

            Busy = false;
            SetReady(true);
        }

        private IEnumerator WaitForMissilesUpdates(int nbMissiles)
        {
            int nbUpdated;
            do
            {
                yield return null;
                nbUpdated = GetAllMissiles().Count(mv => mv.missile.updatedAtTurn == Turn);
            } while (nbUpdated < nbMissiles);

            SetReady(true);
        }

        private IEnumerator WaitForBeamsAnimation(List<Tuple<Vector3, Vector3>> animPositions)
        {
            Busy = animPositions.Any();

            animPositions.ForEach(t =>
            {
                var (from, to) = t;
                beamsLines.AddLine(from, to);
            });

            var duration = animPositions.Any() ? GameSettings.Default.BeamDuration : 0;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            beamsLines.RemoveLines();

            Busy = false;

            photonView.RPC("RPC_ReadyToContinue", RpcTarget.MasterClient);
        }

        private IEnumerator AnimateMoveMissilesToNextPosition()
        {
            var missiles = GetAllMissiles();

            if (missiles.Any())
            {
                Busy = true;

                missiles.ForEach(missileView => missileView.AutoMove());

                do
                {
                    yield return null;
                } while (missiles.Any(s => s.Busy));

                Busy = false;
            }

            photonView.RPC("RPC_ReadyToContinue", RpcTarget.MasterClient);
        }

        public void DisengageSelectedShip()
        {
            if (SelectedShip != null && SelectedShip.OwnedByClient && SelectedShip.ship.Status == ShipStatus.Ok)
            {
                SelectedShip.GetComponent<ShipLog>().AddReport(new Report()
                {
                    turn = Turn,
                    type = ReportType.ShipDisengaged,
                    message = $"{SelectedShip.ship.name} has disengaged"
                });

                if (camera.settings.lockTargetTransform == SelectedShip.transform)
                {
                    camera.settings.cameraLocked = false;
                    camera.settings.lockTargetTransform = null;
                }

                SelectedShip.Disengage();
                OnShipStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SurrenderSelectedShip()
        {
            if (SelectedShip != null && SelectedShip.OwnedByClient && SelectedShip.ship.Status == ShipStatus.Ok)
            {
                SelectedShip.GetComponent<ShipLog>().AddReport(new Report()
                {
                    turn = Turn,
                    type = ReportType.ShipSurrendered,
                    message = $"{SelectedShip.ship.name} surrendered"
                });

                SelectedShip.Surrender();
                OnShipStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AttemptRepair(SsdAlteration alteration)
        {
            photonView.RPC("RPC_AttemptRepair", RpcTarget.MasterClient, SelectedShip.ship.uid,
                (int) alteration.location, alteration.side, alteration.type, alteration.slotType);
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

        public static List<MissileView> GetAllMissiles()
        {
            return PhotonNetwork
                .FindGameObjectsWithComponent(typeof(MissileView))
                .Select(go => go.GetComponent<MissileView>())
                .ToList();
        }

        public static void SetReady(bool ready)
        {
            PhotonNetwork.LocalPlayer.SetReady(ready);
        }

        #endregion AllClients
    }
}