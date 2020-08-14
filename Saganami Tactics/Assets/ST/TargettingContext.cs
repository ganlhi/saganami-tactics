using System;
using ST.Scriptable;
using UnityEngine;

namespace ST
{
    [Serializable]
    public struct TargetingContext
    {
        public override bool Equals(object obj)
        {
            return obj is TargetingContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Side;
                hashCode = (hashCode * 397) ^ Mount.GetHashCode();
                hashCode = (hashCode * 397) ^ Target.GetHashCode();
                return hashCode;
            }
        }

        public Side Side;
        public WeaponMount Mount;
        public int Number;
        public Ship Target;
        public Vector3 LaunchPoint;
        public float LaunchDistance;

        private bool Equals(TargetingContext other)
        {
            return Side == other.Side && Mount.Equals(other.Mount) && Target.Equals(other.Target);
        }
        
        public static bool operator ==(TargetingContext first, TargetingContext second) 
        {
            return Equals(first, second);
        }
        
        public static bool operator !=(TargetingContext first, TargetingContext second) 
        {
            return !(first == second);
        }
    }
}