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

        private List<TargetingContext> _potentialTargets = new List<TargetingContext>();

        private readonly Dictionary<WeaponMount, TargetingContext> _locks =
            new Dictionary<WeaponMount, TargetingContext>();

        public List<TargetingContext> PotentialTargets
        {
            get => _potentialTargets;
            set
            {
                _potentialTargets = value;
                _locks.Clear();
            }
        }

        public ReadOnlyDictionary<WeaponMount, TargetingContext> Locks =>
            new ReadOnlyDictionary<WeaponMount, TargetingContext>(_locks);

        private void Awake()
        {
            _shipView = GetComponent<ShipView>();
        }

        public void Clear()
        {
            photonView.RPC("RPC_Clear", RpcTarget.All);
        }

        public void LockTarget(WeaponMount mount, TargetingContext targetingContext)
        {
            var mountIndex = Array.FindIndex(_shipView.ship.Ssd.weaponMounts, weaponMount => weaponMount.Equals(mount));
            photonView.RPC("RPC_LockTarget", RpcTarget.All,
                targetingContext.Side,
                mountIndex,
                targetingContext.Number,
                targetingContext.Target.uid,
                targetingContext.LaunchPoint,
                targetingContext.LaunchDistance
            );
        }

        public void UnlockTarget(WeaponMount mount)
        {
            var mountIndex = Array.FindIndex(_shipView.ship.Ssd.weaponMounts, weaponMount => weaponMount.Equals(mount));
            photonView.RPC("RPC_UnlockTarget", RpcTarget.All, mountIndex);
        }

        [PunRPC]
        private void RPC_Clear()
        {
            _potentialTargets.Clear();
            _locks.Clear();
        }

        [PunRPC]
        private void RPC_LockTarget(Side side, int mountIndex, int number, string targetId, Vector3 launchPoint,
            float launchDistance)
        {
            var mount = _shipView.ship.Ssd.weaponMounts[mountIndex];
            var target = GameManager.GetShipById(targetId);

            if (target == null) return;

            var targetingContext = new TargetingContext()
            {
                Mount = mount,
                Number = number,
                Side = side,
                Target = target.ship,
                LaunchPoint = launchPoint,
                LaunchDistance = launchDistance
            };

            _locks.Add(mount, targetingContext);
        }

        [PunRPC]
        private void RPC_UnlockTarget(int mountIndex)
        {
            var mount = _shipView.ship.Ssd.weaponMounts[mountIndex];
            _locks.Remove(mount);
        }
    }
}