using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using ST.Common;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    enum PostSyncAction
    {
        None,
        PlaceMarker,
        MarkReadyToMove,
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

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SyncShip(PostSyncAction.None);
            }
        }

        #region MasterClient

        private void SyncShip(PostSyncAction andThen)
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
            SyncShip(PostSyncAction.PlaceMarker);
        }

        public void PlaceMarker()
        {
            ship.PlaceMarker();
            SyncShip(andThen: PostSyncAction.PlaceMarker);
        }

        public void ResetThrustAndPlottings()
        {
            ship.ResetThrustAndPlottings();
            SyncShip(andThen: PostSyncAction.PlaceMarker);
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

            ship.ApplyDisplacement(); // TODO delay for non owners?
            SyncShip(andThen: PostSyncAction.PlaceMarker);
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
            PostSyncAction andThen)
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
                case PostSyncAction.None:
                    break;
                case PostSyncAction.PlaceMarker:
                    var emTransform = EndMarker.transform;
                    emTransform.position = ship.endMarkerPosition;
                    emTransform.rotation = ship.endMarkerRotation;
                    EndMarker.gameObject.SetActive(transform.position != ship.endMarkerPosition);
                    break;
                case PostSyncAction.MarkReadyToMove:
                    MarkReadyToMove();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(andThen), andThen, null);
            }
        }

        private void MarkReadyToMove()
        {
            var shouldMarkItReady = false;
            if (OwnedByClient)
                shouldMarkItReady = true;
            else
            {
                // If no player control this ship and we are the master, let's mark it ready
                if (PhotonNetwork.CurrentRoom.Players.Values.All(p => p.GetTeam() != ship.team) &&
                    PhotonNetwork.IsMasterClient)
                    shouldMarkItReady = true;
            }

            if (shouldMarkItReady)
                photonView.RPC("RPC_SetReadyToMove", RpcTarget.All, true);
        }

        public void AutoMove()
        {
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

        #endregion AllClients
    }
}