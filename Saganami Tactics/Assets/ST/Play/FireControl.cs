using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class FireControl : MonoBehaviourPun
    {
        private ShipView _shipView;

        private List<TargettingContext> _potentialTargets = new List<TargettingContext>();

        private readonly Dictionary<WeaponMount, TargettingContext> _locks =
            new Dictionary<WeaponMount, TargettingContext>();

        public List<TargettingContext> PotentialTargets
        {
            get => _potentialTargets;
            set
            {
                _potentialTargets = value;
                _locks.Clear();
            }
        }

        public ReadOnlyDictionary<WeaponMount, TargettingContext> Locks =>
            new ReadOnlyDictionary<WeaponMount, TargettingContext>(_locks);

        private void Awake()
        {
            _shipView = GetComponent<ShipView>();
        }

        public void Clear()
        {
            _potentialTargets.Clear();
            _locks.Clear();
        }

        public void LockTarget(WeaponMount mount, TargettingContext targettingContext)
        {
            var mountIndex = Array.FindIndex(_shipView.ship.Ssd.weaponMounts, weaponMount => weaponMount.Equals(mount));
            photonView.RPC("RPC_LockTarget", RpcTarget.All,
                targettingContext.Side,
                mountIndex,
                targettingContext.Number,
                targettingContext.Target.uid,
                targettingContext.LaunchPoint,
                targettingContext.LaunchDistance
            );
        }

        public void UnlockTarget(WeaponMount mount)
        {
            var mountIndex = Array.FindIndex(_shipView.ship.Ssd.weaponMounts, weaponMount => weaponMount.Equals(mount));
            photonView.RPC("RPC_UnlockTarget", RpcTarget.All, mountIndex);
        }

        [PunRPC]
        private void RPC_LockTarget(Side side, int mountIndex, int number, string targetId, Vector3 launchPoint,
            float launchDistance)
        {
            var mount = _shipView.ship.Ssd.weaponMounts[mountIndex];
            var target = GameManager.GetShipById(targetId);

            if (target == null) return;

            var targettingContext = new TargettingContext()
            {
                Mount = mount,
                Number = number,
                Side = side,
                Target = target.ship,
                LaunchPoint = launchPoint,
                LaunchDistance = launchDistance
            };

            _locks.Add(mount, targettingContext);
        }

        [PunRPC]
        private void RPC_UnlockTarget(int mountIndex)
        {
            var mount = _shipView.ship.Ssd.weaponMounts[mountIndex];
            _locks.Remove(mount);
        }
    }
}