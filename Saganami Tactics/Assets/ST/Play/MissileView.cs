using System;
using System.Collections;
using Photon.Pun;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    internal enum MissilePostSyncAction
    {
        None,
    }

    [RequireComponent(typeof(PhotonView))]
    public class MissileView : MonoBehaviourPun
    {
        public Missile missile;

        public bool Busy { get; private set; }

        private ShipView _attacker;
        private ShipView _target;
        
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SyncMissile(MissilePostSyncAction.None);
            }
        }

        #region MasterClient

        public void UpdateMissile(Missile updatedMissile)
        {
            missile = updatedMissile;
            SyncMissile(andThen: MissilePostSyncAction.None);
        }
        
        private void SyncMissile(MissilePostSyncAction andThen)
        {
            photonView.RPC("RPC_SetMissile", RpcTarget.All,
                missile.uid,
                missile.attackerId,
                missile.targetId,
                missile.launchPoint,
                missile.number,
                missile.weapon.name,
                missile.position,
                missile.rotation,
                missile.status,
                andThen);
        }

        #endregion MasterClient

        #region AllClients

        [PunRPC]
        private void RPC_SetMissile(
            string uid,
            string attackerId,
            string targetId,
            Vector3 launchPoint,
            int number,
            string weaponName,
            Vector3 position,
            Quaternion rotation,
            MissileStatus status,
            MissilePostSyncAction andThen)
        {
            missile.uid = uid;
            missile.attackerId = attackerId;
            missile.targetId = targetId;
            missile.launchPoint = launchPoint;
            missile.number = number;
            missile.weapon = WeaponHelper.GetWeaponByName(weaponName);
            missile.position = position;
            missile.rotation = rotation;
            missile.status = status;

            _attacker = GameManager.GetShipById(attackerId);
            _target = GameManager.GetShipById(targetId);
            
            switch (andThen)
            {
                case MissilePostSyncAction.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(andThen), andThen, null);
            }
        }

        public void AutoMove()
        {
            StartCoroutine(MakeMovement());
        }

        private IEnumerator MakeMovement()
        {
            var missileTransform = transform;
            var startPos = missileTransform.position;

            var toPos = missile.nextMovePosition;
            
            var startRot = missileTransform.rotation;
            var dir = toPos - startPos;
            var toRot = Quaternion.LookRotation(dir);

            const float duration = 2f;
            const float rotDuration = .5f; // missiles rotate very quickly!
            var elapsedTime = 0f;

            Busy = true;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, toPos, elapsedTime / duration);
                transform.rotation = Quaternion.Lerp(startRot, toRot, Mathf.Min(1f, elapsedTime / rotDuration));
                yield return null;
            }

            Busy = false;
        }

        #endregion AllClients
    }
}