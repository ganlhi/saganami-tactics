using System.Collections.Generic;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class FireControl : MonoBehaviour
    {
        private List<TargettingContext> _potentialTargets = new List<TargettingContext>();

        public List<TargettingContext> PotentialTargets
        {
            get => _potentialTargets;
            set
            {
                _potentialTargets = value;
                Locks.Clear();
            }
        }

        public Dictionary<WeaponMount, TargettingContext> Locks { get; private set; } =
            new Dictionary<WeaponMount, TargettingContext>();
    }
}