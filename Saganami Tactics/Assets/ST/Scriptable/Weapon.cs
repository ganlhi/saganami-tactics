using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        public int span = 1;

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
        
        private bool Equals(Weapon other)
        {
            return base.Equals(other) && string.Equals(name, other.name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Weapon) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (name != null ? name.GetHashCode() : 0);
            }
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
        Graser,
        // TODO add other types needing special rules
    }

    public static class WeaponHelper
    {
        private static Dictionary<string, Weapon> _weapons;

        [CanBeNull]
        public static Weapon GetWeaponByName(string name)
        {
            if (_weapons == null)
            {
                var weapons = Resources.LoadAll<Weapon>("SSD/Weapons");
                _weapons = weapons.ToDictionary((w) => w.name);
            }

            return _weapons.TryGetValue(name, out var weapon) ? weapon : null;
        }

        public static bool IsBeamWeapon(Weapon weapon)
        {
            switch (weapon.type)
            {
                case WeaponType.Missile:
                    return false;
                case WeaponType.Laser:
                case WeaponType.Graser:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsProjectileWeapon(Weapon weapon)
        {
            switch (weapon.type)
            {
                case WeaponType.Missile:
                    return true;
                case WeaponType.Laser:
                case WeaponType.Graser:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}