using System;
using System.Linq;
using UnityEngine;

namespace ST.Scriptable
{
    [CreateAssetMenu(menuName = "ST/Weapon")]
    public class Weapon : ScriptableObject
    {
        public new string name;
        public WeaponType type;
        public int evasion;
        public RangeBand[] bands;

        public int GetMaxRange()
        {
            return bands.Max(b => b.to);
        }

        public RangeBand? GetRangeBand(int distance)
        {
            var okBands = bands.Where(rb => rb.to >= distance && rb.from <= distance).ToList();
            if (!okBands.Any())
            {
                return null;
            }

            return okBands.First();
        }
    }

    [Serializable]
    public struct RangeBand
    {
        public int from;
        public int to;
        public int accuracy;
        public int damage;
        public int penetration;
    }

    public enum WeaponType
    {
        Missile,
        Laser,
        // TODO add other types needing special rules
    }
}