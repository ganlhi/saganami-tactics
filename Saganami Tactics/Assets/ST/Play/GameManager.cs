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

        public event EventHandler<bool> OnBusyChange;
        public event EventHandler<int> OnTurnChange;
        public event EventHandler<TurnStep> OnTurnStepChange;
        public event EventHandler OnShipsInit;
        public event EventHandler<ShipView> OnSelectShip;
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
        private int _clientsReadyToContinue;
        private Dictionary<ShipView, List<Report>> _pendingReports = new Dictionary<ShipView, List<Report>>();

        private Dictionary<ShipView, List<SsdAlteration>> _pendingAlterations =
            new Dictionary<ShipView, List<SsdAlteration>>();

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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void PopulatePendingReportsAndAlterations(ShipView target,
            IEnumerable<Tuple<ReportType, string>> reports,
            IEnumerable<SsdAlteration> pendingAlterations)
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

            if (_pendingAlterations.ContainsKey(target))
                _pendingAlterations[target].AddRange(pendingAlterations);
            else
                _pendingAlterations.Add(target, pendingAlterations.ToList());
        }

        private void FireMissiles()
        {
            GetAllShips().ForEach(shipView =>
            {
                var fcon = shipView.GetComponent<FireControl>();

                foreach (var targetingContext in fcon.Locks.Values)
                {
                    if (targetingContext.Mount.model.type != WeaponType.Missile) continue;

                    var missile = new Missile(shipView.ship, targetingContext, Turn);

                    var mv = PhotonNetwork
                        .InstantiateSceneObject("Prefabs/MissileView", missile.position, missile.rotation)
                        .GetComponent<MissileView>();

                    mv.missile = missile;

                    // TODO consume ammo from weapon mount
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
                    if (targetingContext.Mount.model.type != WeaponType.Laser) continue;
                    var target = GetShipById(targetingContext.Target.uid);

                    if (target == null || target.ship.Status != ShipStatus.Ok) continue;

                    var reports = new List<Tuple<ReportType, string>>();
                    var pendingAlterations = new List<SsdAlteration>();
                    var animTargetPos = Game.FireBeam(targetingContext, shipView.ship, target.ship,
                        ref reports,
                        ref pendingAlterations);

                    beamAnims.Add(new Tuple<Vector3, Vector3>(shipView.ship.position, animTargetPos));

                    PopulatePendingReportsAndAlterations(target, reports, pendingAlterations);
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

                var pendingAlterations = new List<SsdAlteration>();
                if (missile.status == MissileStatus.Hitting && missile.number > 0)
                {
                    Game.HitTarget(missile.weapon, missile.hitSide, missile.number, missile.attackRange, attacker.ship,
                        target.ship,
                        ref reports, ref pendingAlterations);
                }

                PopulatePendingReportsAndAlterations(target, reports, pendingAlterations);
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
                kv.Value.ForEach(report => kv.Key.GetComponent<ShipLog>().AddReport(report));
            }

            _pendingReports.Clear();
        }

        private void DispatchPendingAlterations()
        {
            foreach (var kv in _pendingAlterations)
            {
                kv.Key.AddAlterations(kv.Value);
//                kv.Value.ForEach(alteration => kv.Key.AddAlteration(alteration));
            }

            _pendingAlterations.Clear();

            CheckShipsForDestruction();
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

        #endregion MasterClient

        #region AllClients

#pragma warning disable 0649
        [SerializeField] private BeamsLines beamsLines;
#pragma warning restore 0649

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

        public void LockCameraToSelectedShip()
        {
            LockCameraToShip(_selectedShip);
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
            var ships = GetAllShips()
                .Where(s => s.ship.Status == ShipStatus.Ok || s.ship.Status == ShipStatus.Surrendered)
                .ToList();

            Busy = true;

            if (ships.Any(s => s.ship.endMarkerPosition != s.ship.position))
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