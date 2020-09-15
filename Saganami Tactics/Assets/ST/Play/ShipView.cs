using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using ST.Common;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    internal enum ShipPostSyncAction
    {
        None,
        PlaceMarker,
        PlaceMarkerIfOwned,
    }

    [RequireComponent(typeof(PhotonView))]
    public class ShipView : MonoBehaviourPun
    {
        public Ship ship;

        public bool OwnedByClient => PhotonNetwork.LocalPlayer.GetTeam() == ship.team;

        public bool Busy { get; private set; }

        public event EventHandler OnAlterationsChange;
        public event EventHandler OnConsumedAmmo;
        public event EventHandler OnAttemptedRepair;
        public event EventHandler<ShipStatus> OnStatusChange;

        private ShipMarker _endMarker;

        public ShipMarker EndMarker
        {
            get
            {
                if (_endMarker != null)
                {
                    return _endMarker;
                }

                _endMarker = GetOrCreateMarker();
                return _endMarker;
            }
        }

#pragma warning disable 0649
        [SerializeField] private GameObject shipObject;
        [SerializeField] private GameObject[] wedges;
        [SerializeField] private GameObject[] vectorsAndMarkers;
        [SerializeField] private ParticleSystem explosion;
#pragma warning restore 0649

        private ShipStatus? _lastCheckedStatus = null;

        private void Start()
        {
            ScaleShipModel();
            
            if (PhotonNetwork.IsMasterClient)
            {
                SyncShip(ShipPostSyncAction.None);
            }
        }

        private void ScaleShipModel()
        {
            var cat = ship.Ssd.category;
            var scale = GameSettings.Default.shipsScales.FirstOrDefault(s => s.category == cat).scale;
            if (Math.Abs(scale) < float.Epsilon || Math.Abs(scale - 1) < float.Epsilon) return;
            shipObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        #region MasterClient

        private void SyncShip(ShipPostSyncAction andThen)
        {
            photonView.RPC("RPC_SetShip", RpcTarget.All,
                ship.uid,
                ship.name,
                ship.team,
                ship.ssdName,
                ship.position,
                ship.rotation,
                ship.velocity,
                ship.endMarkerPosition,
                ship.endMarkerRotation,
                ship.halfRotation,
                ship.Status,
                ship.Yaw,
                ship.Pitch,
                ship.Roll,
                ship.Thrust,
                ship.deployedDecoy,
                ship.repairAttempts.ToArray(),
                andThen);
        }

        public void UpdateFutureMovement()
        {
            ship.UpdateFutureMovement();
            SyncShip(ShipPostSyncAction.PlaceMarker);
        }

        public void PlaceMarker()
        {
            ship.PlaceMarker();
            SyncShip(andThen: ShipPostSyncAction.PlaceMarker);
        }

        public void ResetThrustAndPlottings()
        {
            ship.ResetThrustAndPlottings();
            SyncShip(andThen: ShipPostSyncAction.PlaceMarker);
        }

        public void ResetRepairAttempts()
        {
            ship.ResetRepairAttempts();
            SyncShip(andThen: ShipPostSyncAction.None);
        }

        public void ResetDeployedDecoys()
        {
            ship.deployedDecoy = false;
            SyncShip(andThen: ShipPostSyncAction.None);
        }

        [PunRPC]
        public void RPC_Plot(PlottingAction action, int value)
        {
            switch (action)
            {
                case PlottingAction.Yaw:
                    ship.Yaw += value;
                    break;
                case PlottingAction.Pitch:
                    ship.Pitch += value;
                    break;
                case PlottingAction.Roll:
                    ship.Roll += value;
                    break;
                case PlottingAction.SetThrust:
                    ship.Thrust = value;
                    break;
                case PlottingAction.ResetPivot:
                    ship.Yaw = 0;
                    ship.Pitch = 0;
                    break;
                case PlottingAction.ResetRoll:
                    ship.Roll = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            ship.ApplyDisplacement();
            SyncShip(andThen: ShipPostSyncAction.PlaceMarkerIfOwned);
        }

        public void AddAlteration(SsdAlteration alteration)
        {
            photonView.RPC("RPC_AddAlteration", RpcTarget.All,
                alteration.destroyed,
                (int) alteration.location,
                alteration.side,
                alteration.type,
                alteration.slotType
            );
        }

        public void AddAlterations(List<SsdAlteration> alterations)
        {
            var nb = alterations.Count;
            var data = new object[nb * 5];

            var i = 0;
            foreach (var alteration in alterations)
            {
                data[i] = alteration.destroyed;
                data[i + 1] = (int) alteration.location;
                data[i + 2] = alteration.side;
                data[i + 3] = alteration.type;
                data[i + 4] = alteration.slotType;

                i += 5;
            }

            photonView.RPC("RPC_AddAlterations", RpcTarget.All, nb, data);
        }

        public void RemoveAlteration(int location, Side side, SsdAlterationType type, HitLocationSlotType slotType)
        {
            photonView.RPC("RPC_RemoveAlteration", RpcTarget.All,
                location,
                side,
                type,
                slotType
            );
        }

        public void SetAlterationDestroyed(int location, Side side, SsdAlterationType type,
            HitLocationSlotType slotType)
        {
            photonView.RPC("RPC_SetAlterationDestroyed", RpcTarget.All,
                location,
                side,
                type,
                slotType
            );
        }

        public void ConsumeAmmo(WeaponMount weaponMount, int number)
        {
            var wmIndex = Array.FindIndex(ship.Ssd.weaponMounts, m => m.Equals(weaponMount));
            photonView.RPC("RPC_ConsumeAmmo", RpcTarget.All, wmIndex, number);
        }

        public void ConsumeAmmos(Dictionary<WeaponMount, int> consumedAmmosByWeapon)
        {
            var nb = consumedAmmosByWeapon.Count;
            var data = new int[nb * 2];

            var i = 0;
            foreach (var kv in consumedAmmosByWeapon)
            {
                data[i] = Array.FindIndex(ship.Ssd.weaponMounts, m => m.Equals(kv.Key));
                data[i + 1] = kv.Value;

                i += 2;
            }

            photonView.RPC("RPC_ConsumeAmmos", RpcTarget.All, nb, data);
        }

        [PunRPC]
        private void RPC_Disengage()
        {
            ship.Status = ShipStatus.Disengaged;
            SyncShip(andThen: ShipPostSyncAction.None);
        }

        [PunRPC]
        private void RPC_Surrender()
        {
            ship.Status = ShipStatus.Surrendered;
            SyncShip(andThen: ShipPostSyncAction.None);
        }

        public void DestroyShip()
        {
            ship.Status = ShipStatus.Destroyed;
            SyncShip(andThen: ShipPostSyncAction.None);
        }

        public void AddRepairAttempt(bool successful)
        {
            photonView.RPC("RPC_AddRepairAttempt", RpcTarget.All, successful);
        }

        [PunRPC]
        private void RPC_DeployDecoy()
        {
            var remaining = SsdHelper.GetRemainingDecoys(ship.Ssd, ship.alterations);
            if (remaining <= 0 || ship.deployedDecoy) return;

            ship.deployedDecoy = true;
            SyncShip(andThen: ShipPostSyncAction.None);

            AddAlteration(new SsdAlteration()
            {
                destroyed = true,
                type = SsdAlterationType.Slot,
                slotType = HitLocationSlotType.Decoy,
                location = 1 + (int) Array.FindIndex(ship.Ssd.hitLocations,
                               loc => loc.slots.Any(slot => slot.type == HitLocationSlotType.Decoy))
            });
        }

        #endregion MasterClient

        #region AllClients

        [PunRPC]
        private void RPC_SetShip(
            string uid,
            string shipName,
            Team team,
            string ssdName,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            Vector3 endMarkerPosition,
            Quaternion endMarkerRotation,
            Quaternion halfRotation,
            ShipStatus status,
            int yaw,
            int pitch,
            int roll,
            int thrust,
            bool deployedDecoy,
            bool[] repairAttempts,
            ShipPostSyncAction andThen)
        {
            if (ship.uid == string.Empty)
            {
                ship = new Ship(shipName, team, ssdName, uid);
            }

            ship.position = position;
            ship.rotation = rotation;
            ship.velocity = velocity;
            ship.endMarkerPosition = endMarkerPosition;
            ship.endMarkerRotation = endMarkerRotation;
            ship.halfRotation = halfRotation;
            ship.Status = status;
            ship.Yaw = yaw;
            ship.Pitch = pitch;
            ship.Roll = roll;
            ship.Thrust = thrust;
            ship.deployedDecoy = deployedDecoy;
            ship.repairAttempts = repairAttempts.ToList();

            switch (andThen)
            {
                case ShipPostSyncAction.None:
                    break;
                case ShipPostSyncAction.PlaceMarker:
                    UpdateMarkerTransform();
                    break;
                case ShipPostSyncAction.PlaceMarkerIfOwned:
                    if (OwnedByClient) UpdateMarkerTransform();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(andThen), andThen, null);
            }

            if (!_lastCheckedStatus.HasValue || _lastCheckedStatus.Value != ship.Status)
            {
                OnStatusChange?.Invoke(this, ship.Status);
                UpdateGraphicsBasedOnStatus();
                _lastCheckedStatus = ship.Status;
            }
        }

        private void UpdateGraphicsBasedOnStatus()
        {
            if (ship.Status != ShipStatus.Ok)
            {
                foreach (var go in vectorsAndMarkers)
                {
                    go.SetActive(false);
                }

                EndMarker.gameObject.SetActive(false);
            }

            switch (ship.Status)
            {
                case ShipStatus.Disengaged:
                    GetComponent<BoxCollider>().enabled = false; // avoid hover detection
                    StartCoroutine(EscapeFromView());
                    break;
                case ShipStatus.Surrendered:
                    foreach (var go in wedges)
                    {
                        go.SetActive(false);
                    }

                    break;
                case ShipStatus.Ok:
                    break;
                case ShipStatus.Destroyed:
                    GetComponent<BoxCollider>().enabled = false; // avoid hover detection
                    shipObject.SetActive(false);
                    explosion.Play();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator EscapeFromView()
        {
            var escapeVelocity = transform.forward * 100f;
            var cam = Camera.main;
            var rend = shipObject.GetComponentInChildren<Renderer>();
            do
            {
                transform.position += escapeVelocity * Time.deltaTime;
                yield return null;
            } while (rend.isVisible && cam != null && cam.transform.position.DistanceTo(transform.position) < 300);

            shipObject.SetActive(false);
        }

        private void UpdateMarkerTransform()
        {
            var emTransform = EndMarker.transform;
            var shipTransform = transform;

            emTransform.position = ship.endMarkerPosition;
            emTransform.rotation = ship.endMarkerRotation;

            EndMarker.gameObject.SetActive(
                ship.Status == ShipStatus.Ok &&
                (shipTransform.position != ship.endMarkerPosition || shipTransform.rotation != ship.endMarkerRotation)
            );
        }

        public void AutoMove()
        {
            // Make sure the marker view is updated
            UpdateMarkerTransform();

            StartCoroutine(MakeMovement());
        }

        private IEnumerator MakeMovement()
        {
            var shipTransform = transform;

            var fromPos = shipTransform.position;
            var fromRot = shipTransform.rotation;

            var toPos = ship.endMarkerPosition;
            var toRot = ship.endMarkerRotation;
            var halfRot = ship.halfRotation;

            if (fromPos == toPos && fromRot == toRot) yield break;

            var elapsedTime = 0f;
            var duration = GameSettings.Default.MoveDuration;

            Busy = true;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var timeRatio = elapsedTime / duration;

                transform.position = Vector3.Lerp(fromPos, toPos, timeRatio);

                transform.rotation = timeRatio <= .5f
                    ? Quaternion.Slerp(fromRot, halfRot, timeRatio * 2f)
                    : Quaternion.Slerp(halfRot, toRot, (timeRatio - .5f) * 2f);

                yield return null;
            }

            Busy = false;
        }

        private ShipMarker GetOrCreateMarker()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/ShipEndMarker");
            var marker = Instantiate(prefab, ship.endMarkerPosition, ship.endMarkerRotation)
                .GetComponent<ShipMarker>();

            marker.shipView = this;
            marker.ownedByClient = OwnedByClient;
            marker.gameObject.name = "ShipMarker - " + ship.name;
            marker.gameObject.SetActive(false);

            return marker;
        }

        public void Plot(PlottingAction action, int value)
        {
            photonView.RPC("RPC_Plot", RpcTarget.MasterClient, action, value);
        }


        [PunRPC]
        private void RPC_AddAlterations(int nb, object[] data)
        {
            for (var i = 0; i < nb * 5; i += 5)
            {
                // Check if current chunk fits into data
                if (data.Length <= i + 4) continue;

                var destroyed = (bool) data[i];
                var location = (int) data[i + 1];
                var side = (Side) data[i + 2];
                var type = (SsdAlterationType) data[i + 3];
                var slotType = (HitLocationSlotType) data[i + 4];

                ship.alterations.Add(new SsdAlteration()
                {
                    destroyed = destroyed,
                    location = (int) location,
                    side = side,
                    type = type,
                    slotType = slotType
                });
            }

            OnAlterationsChange?.Invoke(this, EventArgs.Empty);
        }

        [PunRPC]
        private void RPC_AddAlteration(bool destroyed, int location, Side side, SsdAlterationType type,
            HitLocationSlotType slotType)
        {
            ship.alterations.Add(new SsdAlteration()
            {
                destroyed = destroyed,
                location = (int) location,
                side = side,
                type = type,
                slotType = slotType
            });

            OnAlterationsChange?.Invoke(this, EventArgs.Empty);
        }

        [PunRPC]
        private void RPC_RemoveAlteration(int location, Side side, SsdAlterationType type, HitLocationSlotType slotType)
        {
            var index = FindRelevantAlterationIndex(location, side, type, slotType, false);
            if (index == -1)
            {
                Debug.LogError(
                    $"Cannot find alteration in {ship.name}: location {location}, side {side}, type {type}, slotType {slotType}");
                return;
            }

            ship.alterations.RemoveAt(index);
            OnAlterationsChange?.Invoke(this, EventArgs.Empty);
        }

        [PunRPC]
        private void RPC_SetAlterationDestroyed(int location, Side side, SsdAlterationType type,
            HitLocationSlotType slotType)
        {
            var index = FindRelevantAlterationIndex(location, side, type, slotType, false);
            if (index == -1)
            {
                Debug.LogError(
                    $"Cannot find alteration in {ship.name}: location {location}, side {side}, type {type}, slotType {slotType}");
                return;
            }

            var alteration = ship.alterations[index];
            alteration.destroyed = true;
            ship.alterations.RemoveAt(index);
            ship.alterations.Add(alteration);
            OnAlterationsChange?.Invoke(this, EventArgs.Empty);
        }

        private int FindRelevantAlterationIndex(int location, Side side, SsdAlterationType type,
            HitLocationSlotType slotType, bool destroyed)
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
                            return ship.alterations.FindIndex(a =>
                                a.type == SsdAlterationType.Slot && a.slotType == slotType && a.side == side &&
                                a.destroyed == destroyed);
                        default:
                            return ship.alterations.FindIndex(a =>
                                a.type == type && a.location == location && a.slotType == slotType &&
                                a.destroyed == destroyed);
                    }

                case SsdAlterationType.Structural:
                    return ship.alterations.FindIndex(a =>
                        a.type == SsdAlterationType.Structural && a.destroyed == destroyed);
                case SsdAlterationType.Movement:
                    return ship.alterations.FindIndex(a =>
                        a.type == SsdAlterationType.Movement && a.destroyed == destroyed);
                case SsdAlterationType.Sidewall:
                    return ship.alterations.FindIndex(a =>
                        a.type == SsdAlterationType.Sidewall && a.side == side && a.destroyed == destroyed);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [PunRPC]
        private void RPC_ConsumeAmmo(int weaponMountIndex, int number)
        {
            UpdateConsumedAmmo(weaponMountIndex, number);
            OnConsumedAmmo?.Invoke(this, EventArgs.Empty);
        }

        [PunRPC]
        private void RPC_ConsumeAmmos(int nb, int[] data)
        {
            for (var i = 0; i < nb * 2; i += 2)
            {
                if (data.Length <= i + 1) continue;
                var weaponMountIndex = data[i];
                var number = data[i + 1];

                UpdateConsumedAmmo(weaponMountIndex, number);
            }

            OnConsumedAmmo?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateConsumedAmmo(int weaponMountIndex, int number)
        {
            var mount = ship.Ssd.weaponMounts[weaponMountIndex];

            if (ship.consumedAmmo.ContainsKey(weaponMountIndex))
            {
                ship.consumedAmmo[weaponMountIndex] =
                    Math.Min(ship.consumedAmmo[weaponMountIndex] + number, mount.ammo);
            }
            else
            {
                ship.consumedAmmo.Add(weaponMountIndex, Math.Min(number, mount.ammo));
            }
        }

        public void Disengage()
        {
            photonView.RPC("RPC_Disengage", RpcTarget.MasterClient);
        }

        public void Surrender()
        {
            photonView.RPC("RPC_Surrender", RpcTarget.MasterClient);
        }

        [PunRPC]
        private void RPC_AddRepairAttempt(bool successful)
        {
            ship.repairAttempts.Add(successful);
            OnAttemptedRepair?.Invoke(this, EventArgs.Empty);
        }

        public void DeployDecoy()
        {
            photonView.RPC("RPC_DeployDecoy", RpcTarget.MasterClient);
        }

        #endregion AllClients
    }
}