using System;
using System.Collections;
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
            if (PhotonNetwork.IsMasterClient)
            {
                SyncShip(ShipPostSyncAction.None);
            }
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
                (int)alteration.location,
                alteration.side,
                alteration.type,
                alteration.slotType
            );
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
            ShipPostSyncAction andThen)
        {
            ship.uid = uid;
            ship.name = shipName;
            ship.team = team;
            ship.ssdName = ssdName;
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
            emTransform.position = ship.endMarkerPosition;
            emTransform.rotation = ship.endMarkerRotation;
            EndMarker.gameObject.SetActive(transform.position != ship.endMarkerPosition && ship.Status == ShipStatus.Ok);
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
        private void RPC_AddAlteration(bool destroyed, int location, Side side, SsdAlterationType type,
            HitLocationSlotType? slotType)
        {
            ship.alterations.Add(new SsdAlteration()
            {
                destroyed = destroyed,
                location = (uint)location,
                side = side,
                type = type,
                slotType = slotType
            });
        }

        public void Disengage()
        {
            photonView.RPC("RPC_Disengage", RpcTarget.MasterClient);   
        }

        public void Surrender()
        {
            photonView.RPC("RPC_Surrender", RpcTarget.MasterClient);   
        }

        #endregion AllClients
    }
}