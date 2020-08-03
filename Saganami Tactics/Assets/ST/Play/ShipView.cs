using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using ST.Common;
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
        public bool ReadyToMove { get; private set; }

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
                ship.thrust,
                ship.endMarkerPosition,
                ship.endMarkerRotation,
                ship.Status,
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

        public void ApplyMovement()
        {
            ReadyToMove = false;
            ship.MoveToMarker();
            SyncShip(andThen: PostSyncAction.MarkReadyToMove);
        }

        public void ResetThrustAndPlottings()
        {
            ship.ResetThrustAndPlottings();
            SyncShip(andThen: PostSyncAction.None);
        }

        [PunRPC]
        private void RPC_SetReadyToMove(bool ready)
        {
            ReadyToMove = ready;
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
            Vector3 thrust,
            Vector3 endMarkerPosition,
            Quaternion endMarkerRotation,
            ShipStatus status,
            PostSyncAction andThen)
        {
            ship.uid = uid;
            ship.name = shipName;
            ship.team = team;
            ship.ssdName = ssdName;
            ship.position = position;
            ship.rotation = rotation;
            ship.velocity = velocity;
            ship.thrust = thrust;
            ship.endMarkerPosition = endMarkerPosition;
            ship.endMarkerRotation = endMarkerRotation;
            ship.Status = status;

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
            var emTransform = EndMarker.transform;

            var fromPos = shipTransform.position;
            var fromRot = shipTransform.rotation;

            var toPos = ship.endMarkerPosition;
            var toRot = ship.endMarkerRotation;

            var elapsedTime = 0f;
            var duration = GameSettings.MoveDuration;

            Busy = true;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(fromPos, toPos, elapsedTime / duration);
                transform.rotation = Quaternion.Lerp(fromRot, toRot, elapsedTime / duration);
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

        #endregion AllClients
    }
}