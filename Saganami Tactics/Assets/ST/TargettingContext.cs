using ST.Scriptable;
using UnityEngine;

namespace ST
{
    public struct TargettingContext
    {
        public Side Side;
        public int MountIndex;
        public WeaponMount Mount;
        public int Number;
        public Ship Target;
        public Vector3 LaunchPoint;
        public float LaunchDistance;
    }
}